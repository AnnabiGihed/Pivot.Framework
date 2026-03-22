using MediatR;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Changed from <c>INotificationHandler&lt;TEvent&gt;</c> to
///              <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c> so
///              that the Domain layer no longer needs to reference MediatR.
/// Purpose     : Contract for domain event handlers compatible with MediatR notifications,
///              while allowing handlers to return a <see cref="Result"/> for composability and diagnostics.
///              MediatR will invoke <see cref="INotificationHandler{TNotification}.Handle"/>,
///              which unwraps the <see cref="DomainEventNotification{TDomainEvent}"/> and
///              delegates to <see cref="HandleWithResultAsync"/>.
/// </summary>
/// <typeparam name="TEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
	where TEvent : IDomainEvent
{
	/// <summary>
	/// Handles the domain event and returns a <see cref="Result"/>.
	/// </summary>
	Task<Result> HandleWithResultAsync(TEvent domainEvent, CancellationToken cancellationToken);

	/// <summary>
	/// MediatR entry point. Unwraps the notification and delegates to <see cref="HandleWithResultAsync"/>.
	/// </summary>
	/// <remarks>
	/// WARNING: The Result returned by HandleWithResultAsync is intentionally discarded in this
	/// MediatR notification path. Failures expressed through the Result pattern will NOT propagate
	/// to the publisher. If you need Result-based error reporting, use HandleWithResultAsync directly
	/// or implement a custom IDomainEventDispatcher that collects Results.
	/// </remarks>
	async Task INotificationHandler<DomainEventNotification<TEvent>>.Handle(
		DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notification);

		// Result is intentionally ignored here to comply with MediatR's notification contract.
		// Your publishing infrastructure (if needed) can call HandleWithResultAsync directly.
		_ = await HandleWithResultAsync(notification.DomainEvent, cancellationToken);
	}
}
