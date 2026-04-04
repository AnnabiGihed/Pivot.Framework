using System.Text;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using Pivot.Framework.Domain.Shared;
using System.Collections.Concurrent;
using Pivot.Framework.Application.Abstractions.Correlation;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.Resilience;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessagePublisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : RabbitMQ implementation of <see cref="IMessagePublisher"/>.
///              Publishes outbox messages to a RabbitMQ exchange with compression,
///              encryption, Polly retry, and circuit breaker resilience policies.
///              Lazily initialises the connection and channel on first publish.
/// </summary>
public class RabbitMQPublisher(
	IOptions<RabbitMQSettings> options,
	IMessageCompressor compressor,
	IMessageEncryptor encryptor,
	MessagingResiliencePolicies resiliencePolicies,
	IOutboxRoutingResolver? routingResolver = null) : IMessagePublisher, IAsyncDisposable
{
	#region Fields
	/// <summary>Cache of declared queues to avoid redundant declarations.</summary>
	protected readonly ConcurrentDictionary<string, bool> _declaredQueues = new();
	/// <summary>RabbitMQ connection and routing settings.</summary>
	protected readonly RabbitMQSettings _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
	/// <summary>Encryptor for message payload encryption (AES-256).</summary>
	protected readonly IMessageEncryptor _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
	/// <summary>Compressor for message payload compression (GZip).</summary>
	protected readonly IMessageCompressor _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
	/// <summary>Polly retry and circuit breaker policies for resilient publishing.</summary>
	protected readonly MessagingResiliencePolicies _resiliencePolicies = resiliencePolicies ?? throw new ArgumentNullException(nameof(resiliencePolicies));
	/// <summary>Optional service-specific resolver for per-message RabbitMQ routing.</summary>
	protected readonly IOutboxRoutingResolver? _routingResolver = routingResolver;

	private IConnection? _connection;
	private IChannel? _channel;
	private readonly SemaphoreSlim _connectionLock = new(1, 1);
	private volatile bool _disposed;
	#endregion

	#region IMessagePublisher Implementation
	/// <summary>
	/// Publishes an outbox message to the configured RabbitMQ exchange.
	/// The message payload is compressed and encrypted before publishing.
	/// Uses Polly circuit breaker and retry policies for resilience.
	/// </summary>
	/// <param name="message">The outbox message to publish.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public async Task<Result> PublishAsync(OutboxMessage message)
	{
		if (message == null)
			return Result.Failure(new Error("MessageNull", "Message cannot be null."));

		try
		{

			return await _resiliencePolicies.CircuitBreakerPolicy.ExecuteAsync(async () =>
			{
				return await _resiliencePolicies.RetryPolicy.ExecuteAsync(async () =>
				{
					await EnsureConnectionAsync();

					var compressedMessage = _compressor.Compress(Encoding.UTF8.GetBytes(message?.Payload ?? string.Empty));
					var encryptedMessage = _encryptor.Encrypt(compressedMessage);

					// Propagate the correlation ID from the outbox message if available,
					// otherwise use the ambient CorrelationContext, falling back to a new GUID.
					var correlationId = message.CorrelationId
						?? CorrelationContext.EnsureCorrelationId();

					var properties = new BasicProperties
					{
						ContentType = "application/octet-stream",
						DeliveryMode = DeliveryModes.Persistent,
						Headers = new Dictionary<string, object?>
						{
							{ "CorrelationId", correlationId },
							{ "EventId", message.Id.ToString() },
							{ "Timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
						},
						Type = message.EventType // Set the fully qualified type name
					};

					var route = ResolveRoute(message);

					await PublishToChannelAsync(route, properties, encryptedMessage);

					return Result.Success();
				});
			});
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("MessagePublishError", "An error occurred while publishing message, with message : "+ ex.Message));
		}
	}
	#endregion

	#region Utilities
	/// <summary>
	/// Lazily creates and caches the RabbitMQ connection and channel.
	/// Exchange and queue topology must be declared separately so publish-time
	/// behavior cannot drift from the startup-time topology contract.
	/// Thread-safe via SemaphoreSlim.
	/// </summary>
	protected virtual async Task EnsureConnectionAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
			return;

		await _connectionLock.WaitAsync();
		try
		{
			// Double-check after acquiring lock
			if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
				return;

			// Clean up stale connection/channel if they exist but are closed
			if (_channel is not null)
			{
				try { _channel.Dispose(); } catch { /* best effort */ }
				_channel = null;
			}
			if (_connection is not null)
			{
				try { _connection.Dispose(); } catch { /* best effort */ }
				_connection = null;
			}

			var factory = new ConnectionFactory
			{
				HostName = _settings.HostName,
				UserName = _settings.UserName,
				Password = _settings.Password,
				VirtualHost = _settings.VirtualHost,
				Port = _settings.Port,
				ClientProvidedName = _settings.ClientProvidedName,
			};

			_connection = await factory.CreateConnectionAsync();
			_channel = await _connection.CreateChannelAsync();
		}
		finally
		{
			_connectionLock.Release();
		}
	}

	/// <summary>
	/// Resolves the publish route for the specified outbox <paramref name="message"/>.
	/// Falls back to <see cref="RabbitMQSettings.Exchange"/> and <see cref="RabbitMQSettings.RoutingKey"/>
	/// when no custom resolver is configured.
	/// </summary>
	protected virtual OutboxRoute ResolveRoute(OutboxMessage message)
	{
		ArgumentNullException.ThrowIfNull(message);

		return _routingResolver?.Resolve(message)
			?? new OutboxRoute(_settings.Exchange, _settings.RoutingKey);
	}

	/// <summary>
	/// Publishes the encrypted payload to the configured channel using the supplied route.
	/// Route-specific topology is expected to be declared separately when the route differs
	/// from the default settings.
	/// </summary>
	protected virtual Task PublishToChannelAsync(OutboxRoute route, BasicProperties properties, byte[] encryptedMessage)
	{
		ArgumentNullException.ThrowIfNull(route);
		ArgumentNullException.ThrowIfNull(properties);
		ArgumentNullException.ThrowIfNull(encryptedMessage);

		return _channel!.BasicPublishAsync(
			route.Exchange,
			route.RoutingKey,
			mandatory: true,
			properties,
			encryptedMessage).AsTask();
	}

	#endregion

	#region IAsyncDisposable Implementation
	/// <summary>
	/// Asynchronously closes and disposes the RabbitMQ channel and connection.
	/// Preferred disposal path that properly awaits the async close operations.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;
		_disposed = true;

		try { if (_channel is not null) await _channel.CloseAsync(); } catch { }
		try { if (_connection is not null) await _connection.CloseAsync(); } catch { }

		_channel?.Dispose();
		_connection?.Dispose();
		_connectionLock?.Dispose();
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
