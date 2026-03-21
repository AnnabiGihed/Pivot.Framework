using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageReceiver;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageReceiver;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Replaced direct MediatR <c>INotification</c> casting with
///              <see cref="IDomainEventDispatcher"/> to decouple message receiving from
///              the dispatching mechanism. Domain events are now dispatched through the
///              application-layer abstraction, preserving Clean Architecture boundaries.
///              Added explicit <c>TypeNameHandling.None</c> to prevent Newtonsoft.Json
///              deserialization attacks.
/// Purpose     : Listens for messages on a RabbitMQ queue, decrypts, decompresses, and
///              deserializes them into domain events, then dispatches them to in-process
///              handlers via <see cref="IDomainEventDispatcher"/>.
/// </summary>
public class RabbitMQReceiver(
	IOptions<RabbitMQSettings> options,
	ILogger<RabbitMQReceiver> logger,
	IMessageCompressor messageCompressor,
	IMessageEncryptor messageEncryptor,
	IServiceProvider serviceProvider) : IMessageReceiver
{
	#region Fields
	/// <summary>
	/// Deserialization settings with <see cref="TypeNameHandling.None"/> explicitly set
	/// to prevent remote code execution via malicious <c>$type</c> payloads.
	/// </summary>
	private static readonly JsonSerializerSettings DeserializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None
	};

	/// <summary>
	/// The RabbitMQ channel used for consuming messages. Initialised by <see cref="InitializeAsync"/>.
	/// </summary>
	protected IChannel? _channel;

	/// <summary>
	/// The RabbitMQ connection. Initialised by <see cref="InitializeAsync"/>.
	/// </summary>
	protected IConnection? _connection;

	/// <summary>
	/// Logger for diagnostic tracing of message receive and dispatch operations.
	/// </summary>
	protected readonly ILogger<RabbitMQReceiver> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// RabbitMQ connection and queue settings loaded from configuration.
	/// </summary>
	protected readonly RabbitMQSettings _settings = options.Value ?? throw new ArgumentNullException(nameof(options));

	/// <summary>
	/// Service provider used to create DI scopes for resolving scoped services per message.
	/// </summary>
	protected readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <summary>
	/// Encryptor used to decrypt incoming message payloads (AES-256).
	/// </summary>
	protected readonly IMessageEncryptor _messageEncryptor = messageEncryptor ?? throw new ArgumentNullException(nameof(messageEncryptor));

	/// <summary>
	/// Compressor used to decompress incoming message payloads (GZip).
	/// </summary>
	protected readonly IMessageCompressor _messageCompressor = messageCompressor ?? throw new ArgumentNullException(nameof(messageCompressor));
	#endregion

	#region IMessageReceiver Implementation
	/// <summary>
	/// Establishes a connection and channel to RabbitMQ and ensures the configured queue exists.
	/// Must be called before <see cref="StartListeningAsync"/>.
	/// </summary>
	public async Task InitializeAsync()
	{
		var factory = new ConnectionFactory
		{
			Port = _settings.Port,
			HostName = _settings.HostName,
			UserName = _settings.UserName,
			Password = _settings.Password,
			VirtualHost = _settings.VirtualHost,
			ClientProvidedName = $"{_settings.ClientProvidedName}-receiver"
		};

		_connection = await factory.CreateConnectionAsync();
		_channel = await _connection.CreateChannelAsync();

		await EnsureQueueExistsAsync();
	}

	/// <summary>
	/// Begins consuming messages from the configured RabbitMQ queue.
	/// Each message is decrypted, decompressed, deserialized into the appropriate
	/// <see cref="IDomainEvent"/> type, and dispatched via a scoped <see cref="IDomainEventDispatcher"/>.
	/// Messages are acknowledged on success and requeued on failure.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown when called before <see cref="InitializeAsync"/>.
	/// </exception>
	public async Task StartListeningAsync()
	{
		if (_connection == null || _channel == null)
		{
			throw new InvalidOperationException("RabbitMQReceiver is not initialized. Call InitializeAsync() first.");
		}

		var consumer = new AsyncEventingBasicConsumer(_channel);
		consumer.ReceivedAsync += async (model, ea) =>
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

				var encryptedMessage = ea.Body.ToArray();
				var compressedMessage = _messageEncryptor.Decrypt(encryptedMessage);
				var messageBytes = _messageCompressor.Decompress(compressedMessage);

				var messagePayload = Encoding.UTF8.GetString(messageBytes);
				_logger.LogInformation("Message received: {Payload}", messagePayload);

				var domainEventType = ea.BasicProperties.Type is not null
					? Type.GetType(ea.BasicProperties.Type)
					: null;
				_logger.LogDebug("Retrieved Domain Event Type: {EventType}", ea.BasicProperties.Type);

				if (domainEventType == null)
				{
					_logger.LogWarning("Unknown event type: {EventType}", ea.BasicProperties.Type);
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
					return;
				}

				var deserialized = JsonConvert.DeserializeObject(messagePayload, domainEventType, DeserializerSettings);

				if (deserialized is not IDomainEvent domainEvent)
				{
					_logger.LogWarning("Deserialized object is not an IDomainEvent: {EventType}", ea.BasicProperties.Type);
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
					return;
				}

				_logger.LogDebug("Dispatching domain event: {EventType} ({EventId})", domainEventType.Name, domainEvent.Id);

				await dispatcher.DispatchAsync(domainEvent);

				await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message. Message will be requeued.");
				await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
			}
		};

		await _channel.BasicConsumeAsync(
			queue: _settings.Queue,
			autoAck: false,
			consumer: consumer);

		_logger.LogInformation("Started listening on queue: {Queue}", _settings.Queue);
	}
	#endregion

	#region Utilities
	/// <summary>
	/// Declares the configured queue on the RabbitMQ channel if it does not already exist.
	/// The queue is created as durable, non-exclusive, and non-auto-delete.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the channel is not initialised.</exception>
	protected async Task EnsureQueueExistsAsync()
	{
		if (_channel == null) throw new InvalidOperationException("Channel is not initialized.");

		await _channel.QueueDeclareAsync(
			queue: _settings.Queue,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null);

		_logger.LogInformation("Ensured queue '{Queue}' exists.", _settings.Queue);
	}
	#endregion

	#region IDisposable Implementation
	/// <summary>
	/// Closes and disposes the RabbitMQ channel and connection.
	/// </summary>
	public void Dispose()
	{
		_channel?.CloseAsync();
		_connection?.CloseAsync();

		_channel?.Dispose();
		_connection?.Dispose();
	}
	#endregion
}
