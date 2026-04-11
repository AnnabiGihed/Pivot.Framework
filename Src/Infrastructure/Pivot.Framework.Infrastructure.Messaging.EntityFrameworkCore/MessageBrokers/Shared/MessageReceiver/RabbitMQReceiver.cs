using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Correlation;
using Pivot.Framework.Application.Abstractions.Causation;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;
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
///              the dispatching mechanism. Added explicit <c>TypeNameHandling.None</c>
///              to prevent Newtonsoft.Json deserialization attacks.
///              Added <see cref="CausationContext"/> propagation: sets CausationId to the
///              incoming event's EventId so downstream events record their causal chain.
///              Added version-namespaced deduplication key support for projection rebuilds.
/// Purpose     : Listens for messages on a RabbitMQ queue, decrypts, decompresses, and
///              deserializes them into domain events, then dispatches them to in-process
///              handlers via <see cref="IDomainEventDispatcher"/>.
///              Implements the inbox pattern for consumer-side idempotency: before dispatching,
///              checks whether the message has already been processed; after successful dispatch,
///              records consumption to prevent duplicate handling on redelivery.
///              Extracts and propagates the correlation ID from message headers into
///              <see cref="CorrelationContext"/> for end-to-end distributed tracing.
/// </summary>
public class RabbitMQReceiver(
	IOptions<RabbitMQSettings> options,
	ILogger<RabbitMQReceiver> logger,
	IMessageCompressor messageCompressor,
	IMessageEncryptor messageEncryptor,
	IServiceProvider serviceProvider) : IMessageReceiver, IAsyncDisposable
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

	private bool _disposed;
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

				// ── Correlation ID propagation ──────────────────────────────────
				// Extract the correlation ID from the incoming message headers and set it
				// on the ambient CorrelationContext so that all downstream handlers,
				// outbox writes, and outgoing messages share the same trace.
				var correlationId = ExtractCorrelationId(ea.BasicProperties);
				CorrelationContext.CorrelationId = correlationId;

				// ── Causation ID propagation ────────────────────────────────────
				// Extract the EventId from the incoming message. When a handler processes
				// this event and produces new events, those new events' CausationId will
				// be set to this EventId, enabling full causal chain reconstruction.
				var incomingEventId = ExtractHeaderValue(ea.BasicProperties, "EventId");
				CausationContext.CausationId = incomingEventId;

				var encryptedMessage = ea.Body.ToArray();
				var compressedMessage = _messageEncryptor.Decrypt(encryptedMessage);
				var messageBytes = _messageCompressor.Decompress(compressedMessage);

				var messagePayload = Encoding.UTF8.GetString(messageBytes);
				if (string.IsNullOrWhiteSpace(messagePayload))
				{
					_logger.LogWarning("Empty message payload for event type: {EventType}. Rejecting to DLQ.", ea.BasicProperties.Type);
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
					return;
				}

				var eventType = ea.BasicProperties.Type;
				_logger.LogDebug("Message received. Type: {EventType}, Size: {Size} bytes, CorrelationId: {CorrelationId}",
					eventType, messagePayload?.Length ?? 0, correlationId);

				var messageEventType = ea.BasicProperties.Type is not null
					? Type.GetType(ea.BasicProperties.Type)
					: null;
				_logger.LogDebug("Retrieved message event type: {EventType}", ea.BasicProperties.Type);

				if (messageEventType == null)
				{
					// Reject without requeue — the message will be routed to the dead-letter queue
					// (if one is configured) or discarded. Requeuing would cause an infinite loop
					// because the event type will never become resolvable without a code change.
					_logger.LogWarning("Unknown event type: {EventType}. Rejecting to DLQ.", ea.BasicProperties.Type);
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
					return;
				}

				var deserialized = JsonConvert.DeserializeObject(messagePayload!, messageEventType, DeserializerSettings);

				if (deserialized is not IDomainEvent and not IIntegrationEvent)
				{
					// Reject without requeue — deserialization failures are deterministic and
					// retrying the same payload will always produce the same result.
					// The message will be routed to the DLQ if one is configured.
					_logger.LogWarning("Deserialized object is neither an IDomainEvent nor an IIntegrationEvent: {EventType}. Rejecting to DLQ.", ea.BasicProperties.Type);
					await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
					return;
				}

				// ── Inbox pattern (consumer-side idempotency) ───────────────────
				// Check whether this message has already been processed by this consumer.
				// If yes, acknowledge without dispatching to prevent duplicate side effects.
				//
				// Supports version-namespaced deduplication keys for projection rebuilds:
				// - During rebuild (ReplayFlag header present): key = "ProjectionVersion:EventId:ConsumerName"
				// - During live processing: key = "EventId:ConsumerName"
				// This ensures replay-era keys don't collide with live keys (MDM spec invariant #29).
				var inboxService = scope.ServiceProvider.GetService<IInboxService>();
				var eventId = ResolveEventId(deserialized);
				if (inboxService is not null)
				{
					var consumerName = _settings.ClientProvidedName ?? "RabbitMQReceiver";
					var deduplicationId = BuildDeduplicationId(eventId, ea.BasicProperties);

					var alreadyProcessed = await inboxService.HasBeenProcessedAsync(deduplicationId, consumerName);

					if (alreadyProcessed)
					{
						_logger.LogInformation(
							"Message {EventId} already processed by consumer {ConsumerName}. Acknowledging without dispatch.",
							eventId, consumerName);
						await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
						return;
					}
				}

				// ── Set CausationId to the incoming event's Id ──────────────────
				// This ensures any events raised by handlers of this event carry
				// the correct CausationId for causal chain reconstruction.
				CausationContext.CausationId = eventId.ToString();

				await DispatchReceivedEventAsync(scope.ServiceProvider, deserialized, messageEventType, eventId);

				// Record consumption in the inbox after successful dispatch.
				if (inboxService is not null)
				{
					var consumerName = _settings.ClientProvidedName ?? "RabbitMQReceiver";
					var deduplicationId = BuildDeduplicationId(eventId, ea.BasicProperties);
					await inboxService.RecordConsumptionAsync(deduplicationId, consumerName);
					await inboxService.SaveChangesAsync();
				}

				await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message {EventType}. Nacking without requeue.", ea.BasicProperties.Type);
				await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
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
	/// Extracts the correlation ID from the incoming message headers.
	/// Falls back to generating a new correlation ID if none is present.
	/// </summary>
	/// <param name="properties">The basic properties of the received message.</param>
	/// <returns>The extracted or generated correlation identifier.</returns>
	private static string ExtractCorrelationId(IReadOnlyBasicProperties properties)
	{
		if (properties.Headers is not null &&
			properties.Headers.TryGetValue("CorrelationId", out var raw))
		{
			// RabbitMQ stores header values as byte arrays
			if (raw is byte[] bytes)
				return Encoding.UTF8.GetString(bytes);

			if (raw is string s)
				return s;
		}

		return Guid.NewGuid().ToString();
	}

	/// <summary>
	/// Resolves the identifier of a received domain or integration event.
	/// </summary>
	/// <param name="eventObject">The deserialized event object.</param>
	/// <returns>The event identifier.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the event object is not a supported event type.</exception>
	private static Guid ResolveEventId(object eventObject)
	{
		return eventObject switch
		{
			IDomainEvent domainEvent => domainEvent.Id,
			IIntegrationEvent integrationEvent => integrationEvent.Id,
			_ => throw new InvalidOperationException("Received message is not a supported event type.")
		};
	}

	/// <summary>
	/// Dispatches a received event through the matching domain or integration event dispatcher.
	/// </summary>
	/// <param name="serviceProvider">The scoped service provider.</param>
	/// <param name="eventObject">The deserialized event object.</param>
	/// <param name="eventType">The resolved event CLR type.</param>
	/// <param name="eventId">The event identifier.</param>
	/// <returns>A task representing the dispatch operation.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the event object is not a supported event type.</exception>
	private async Task DispatchReceivedEventAsync(IServiceProvider serviceProvider, object eventObject, Type eventType, Guid eventId)
	{
		switch (eventObject)
		{
			case IDomainEvent domainEvent:
				var domainEventDispatcher = serviceProvider.GetRequiredService<IDomainEventDispatcher>();
				_logger.LogDebug("Dispatching domain event: {EventType} ({EventId})", eventType.Name, eventId);
				await domainEventDispatcher.DispatchAsync(domainEvent);
				break;

			case IIntegrationEvent integrationEvent:
				var integrationEventDispatcher = serviceProvider.GetRequiredService<IIntegrationEventDispatcher>();
				_logger.LogDebug("Dispatching integration event: {EventType} ({EventId})", eventType.Name, eventId);
				await integrationEventDispatcher.DispatchAsync(integrationEvent);
				break;

			default:
				throw new InvalidOperationException("Received message is not a supported event type.");
		}
	}

	/// <summary>
	/// Extracts a string header value from message properties.
	/// </summary>
	private static string? ExtractHeaderValue(IReadOnlyBasicProperties properties, string headerName)
	{
		if (properties.Headers is not null &&
			properties.Headers.TryGetValue(headerName, out var raw))
		{
			if (raw is byte[] bytes)
				return Encoding.UTF8.GetString(bytes);
			if (raw is string s)
				return s;
		}
		return null;
	}

	/// <summary>
	/// Builds a deduplication ID supporting version-namespaced keys for projection rebuilds.
	/// During rebuild (ProjectionVersion header present): "ProjectionVersion:EventId"
	/// During live processing: uses the raw EventId.
	/// </summary>
	private static Guid BuildDeduplicationId(Guid eventId, IReadOnlyBasicProperties properties)
	{
		var projectionVersion = ExtractHeaderValue(properties, "ProjectionVersion");
		if (!string.IsNullOrEmpty(projectionVersion))
		{
			// Create a deterministic GUID from "ProjectionVersion:EventId" to ensure
			// replay-era dedup keys don't collide with live keys
			var compositeKey = $"{projectionVersion}:{eventId}";
			var bytes = System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(compositeKey));
			return new Guid(bytes);
		}
		return eventId;
	}

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

	#region IAsyncDisposable Implementation
	/// <summary>
	/// Asynchronously closes and disposes the RabbitMQ channel and connection.
	/// Preferred disposal path that properly awaits the async close operations.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
			return;

		_disposed = true;

		try
		{
			if (_channel is not null)
			{
				await _channel.CloseAsync();
				_channel.Dispose();
				_channel = null;
			}
		}
		catch { /* best effort during disposal */ }

		try
		{
			if (_connection is not null)
			{
				await _connection.CloseAsync();
				_connection.Dispose();
				_connection = null;
			}
		}
		catch { /* best effort during disposal */ }

		GC.SuppressFinalize(this);
	}
	#endregion

	#region IDisposable Implementation
	/// <summary>
	/// Synchronous fallback that blocks on the async close operations.
	/// Prefer <see cref="DisposeAsync"/> when possible.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		DisposeAsync().AsTask().GetAwaiter().GetResult();
	}
	#endregion
}
