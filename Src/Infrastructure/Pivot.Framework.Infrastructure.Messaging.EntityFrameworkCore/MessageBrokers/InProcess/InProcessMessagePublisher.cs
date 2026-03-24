using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.InProcess;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : In-process <see cref="IMessagePublisher"/> implementation.
///              Instead of publishing to an external broker (RabbitMQ), deserializes the
///              outbox message back to an <see cref="IDomainEvent"/> and dispatches it
///              locally via <see cref="IDomainEventDispatcher"/> (MediatR).
///
///              Pluggable alternative to <c>RabbitMQPublisher</c> for single-service deployments
///              that don't need an external message broker.
///
///              Works with the configured outbox draining mode.
///              The outbox guarantees at-least-once delivery even without a broker:
///              if the app crashes between <c>SaveChanges</c> and dispatch, the unprocessed
///              outbox message will be retried on next startup.
///
///              Uses <see cref="TypeNameHandling.None"/> for safe deserialization
///              (prevents remote code execution via malicious <c>$type</c> payloads).
/// </summary>
public class InProcessMessagePublisher : IMessagePublisher
{
	#region Fields
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<InProcessMessagePublisher> _logger;

	/// <summary>
	/// Deserialization settings matching the security configuration used by
	/// <c>RabbitMQReceiver</c> — <see cref="TypeNameHandling.None"/> prevents RCE.
	/// </summary>
	private static readonly JsonSerializerSettings DeserializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None
	};
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="InProcessMessagePublisher"/>.
	/// </summary>
	/// <param name="serviceProvider">
	/// The root service provider. A scoped provider is created per dispatch to ensure
	/// scoped services (DbContext, handlers) are correctly resolved and disposed.
	/// </param>
	/// <param name="logger">The logger instance.</param>
	public InProcessMessagePublisher(
		IServiceProvider serviceProvider,
		ILogger<InProcessMessagePublisher> logger)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region IMessagePublisher Implementation
	/// <summary>
	/// Deserializes the outbox message and dispatches it to in-process handlers via
	/// <see cref="IDomainEventDispatcher"/>. All registered <c>INotificationHandler</c>
	/// implementations (including <c>ProjectionHandler</c>) will receive the event.
	/// </summary>
	/// <param name="message">The outbox message to publish. Must not be null.</param>
	/// <returns>
	/// <see cref="Result.Success()"/> when the event was dispatched successfully;
	/// <see cref="Result.Failure(Error)"/> when the event type cannot be resolved
	/// or deserialization fails.
	/// </returns>
	public async Task<Result> PublishAsync(OutboxMessage message)
	{
		ArgumentNullException.ThrowIfNull(message);

		try
		{
			var domainEventType = Type.GetType(message.EventType!);
			if (domainEventType is null)
			{
				_logger.LogWarning(
					"Unknown event type: {EventType}. Message {MessageId} cannot be dispatched in-process.",
					message.EventType, message.Id);
				return Result.Failure(new Error(
					"InProcessPublisher.UnknownEventType",
					$"Cannot resolve event type: {message.EventType}"));
			}

			var deserialized = JsonConvert.DeserializeObject(
				message.Payload!, domainEventType, DeserializerSettings);

			if (deserialized is not IDomainEvent domainEvent)
			{
				_logger.LogWarning(
					"Deserialized object is not an IDomainEvent: {EventType}. Message {MessageId}.",
					message.EventType, message.Id);
				return Result.Failure(new Error(
					"InProcessPublisher.InvalidEvent",
					$"Deserialized object is not an IDomainEvent: {message.EventType}"));
			}

			using var scope = _serviceProvider.CreateScope();
			var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

			_logger.LogDebug(
				"In-process dispatching domain event: {EventType} ({EventId})",
				domainEventType.Name, domainEvent.Id);

			await dispatcher.DispatchAsync(domainEvent);

			return Result.Success();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Error dispatching message {MessageId} (type: {EventType}) in-process.",
				message.Id, message.EventType);

			return Result.Failure(new Error(
				"InProcessPublisher.DispatchFailed",
				ex.Message));
		}
	}
	#endregion

	#region IDisposable Implementation
	/// <summary>
	/// No external connections to dispose — this publisher is purely in-process.
	/// </summary>
	public void Dispose()
	{
		// No external connections to clean up.
	}
	#endregion
}
