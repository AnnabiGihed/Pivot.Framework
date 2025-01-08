using MediatR;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageEncryptor;
using Temlates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageCompressor;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageReceiver;

public class RabbitMQReceiver : IMessageReceiver
{
	protected IChannel? _channel;
	protected IConnection? _connection;
	protected readonly RabbitMQSettings _settings;
	protected readonly ILogger<RabbitMQReceiver> _logger;
	protected readonly IMessageEncryptor _messageEncryptor;
	protected readonly IMessageCompressor _messageCompressor;
	protected readonly IMediator _mediator; // Inject Mediator

	public RabbitMQReceiver(
		IOptions<RabbitMQSettings> options,
		ILogger<RabbitMQReceiver> logger,
		IMessageCompressor messageCompressor,
		IMessageEncryptor messageEncryptor,
		IMediator mediator) // Mediator for handling domain events
	{
		_settings = options.Value ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_messageCompressor = messageCompressor ?? throw new ArgumentNullException(nameof(messageCompressor));
		_messageEncryptor = messageEncryptor ?? throw new ArgumentNullException(nameof(messageEncryptor));
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
	}

	public async Task InitializeAsync()
	{
		var factory = new ConnectionFactory
		{
			HostName = _settings.HostName,
			UserName = _settings.UserName,
			Password = _settings.Password,
			VirtualHost = _settings.VirtualHost,
			Port = _settings.Port,
			ClientProvidedName = $"{_settings.ClientProvidedName}-receiver"
		};

		_connection = await factory.CreateConnectionAsync();
		_channel = await _connection.CreateChannelAsync();

		await EnsureQueueExistsAsync();
	}

	public async Task StartListeningAsync(Func<string, Task> onMessageReceived = null)
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
				var encryptedMessage = ea.Body.ToArray();
				var compressedMessage = _messageEncryptor.Decrypt(encryptedMessage);
				var messageBytes = _messageCompressor.Decompress(compressedMessage);

				var messagePayload = Encoding.UTF8.GetString(messageBytes);

				_logger.LogInformation($"Message received: {messagePayload}");

				// Deserialize the payload into the appropriate domain event
				var domainEventType = Type.GetType(ea.BasicProperties.Type);
				if (domainEventType == null)
				{
					_logger.LogWarning($"Unknown event type: {ea.BasicProperties.Type}");
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
					return;
				}

				var domainEvent = JsonConvert.DeserializeObject(messagePayload, domainEventType) as INotification;
				if (domainEvent == null)
				{
					_logger.LogWarning($"Failed to deserialize message to event type: {ea.BasicProperties.Type}");
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
					return;
				}

				// Use Mediator to handle the domain event
				await _mediator.Publish(domainEvent);

				// Acknowledge the message
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

		_logger.LogInformation($"Started listening on queue: {_settings.Queue}");
	}

	private async Task EnsureQueueExistsAsync()
	{
		if (_channel == null) throw new InvalidOperationException("Channel is not initialized.");

		await _channel.QueueDeclareAsync(
			queue: _settings.Queue,
			durable: true,
			exclusive: false,
			autoDelete: false,
			arguments: null);

		_logger.LogInformation($"Ensured queue '{_settings.Queue}' exists.");
	}

	public void Dispose()
	{
		_channel?.CloseAsync();
		_connection?.CloseAsync();
		_channel?.Dispose();
		_connection?.Dispose();
	}
}
