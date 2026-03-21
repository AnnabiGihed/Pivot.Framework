using MediatR;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.DomainEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : MediatR-based implementation of <see cref="IDomainEventDispatcher"/>.
///              Wraps each <see cref="IDomainEvent"/> in a <see cref="DomainEventNotification{TDomainEvent}"/>
///              and publishes it through MediatR's notification pipeline.
///
///              This implementation preserves Clean Architecture boundaries:
///              - The Domain layer has zero knowledge of MediatR.
///              - The Application layer defines the notification adapter and handler contracts.
///              - This Infrastructure class bridges the two using reflection to construct
///                the generic <see cref="DomainEventNotification{TDomainEvent}"/> at runtime.
/// </summary>
public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
	#region Fields
	/// <summary>
	/// The MediatR mediator used for in-process notification publishing.
	/// </summary>
	private readonly IMediator _mediator;

	/// <summary>
	/// Logger for diagnostic tracing of domain event dispatch operations.
	/// </summary>
	private readonly ILogger<MediatRDomainEventDispatcher> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="MediatRDomainEventDispatcher"/> with the provided dependencies.
	/// </summary>
	/// <param name="mediator">The MediatR mediator. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="mediator"/> or <paramref name="logger"/> is null.
	/// </exception>
	public MediatRDomainEventDispatcher(IMediator mediator, ILogger<MediatRDomainEventDispatcher> logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Dispatches a domain event to all registered handlers by wrapping it in a
	/// <see cref="DomainEventNotification{TDomainEvent}"/> and publishing via MediatR.
	/// The generic wrapper is constructed at runtime using reflection to avoid requiring
	/// the caller to know the concrete event type at compile time.
	/// </summary>
	/// <param name="domainEvent">The domain event to dispatch. Must not be null.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the <see cref="DomainEventNotification{TDomainEvent}"/> could not be instantiated.
	/// </exception>
	public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(domainEvent);

		var eventType = domainEvent.GetType();
		var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
		var notification = Activator.CreateInstance(notificationType, domainEvent)
			?? throw new InvalidOperationException(
				$"Failed to create DomainEventNotification<{eventType.Name}>.");

		_logger.LogDebug(
			"Dispatching domain event {EventType} ({EventId}) via MediatR.",
			eventType.Name, domainEvent.Id);

		await _mediator.Publish(notification, cancellationToken);
	}
	#endregion
}
