using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Application.DomainEventHandler;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Updated to handle <see cref="DomainEventNotification{TEvent}"/>
///              instead of raw <typeparamref name="TEvent"/>, keeping the handler surface clean
///              for subclasses via <see cref="HandleWithResultAsync"/>.
/// Purpose     : Convenience base class for domain event handlers.
///              Subclasses override <see cref="HandleWithResultAsync"/> and never interact
///              with MediatR types directly.
/// </summary>
/// <typeparam name="TEvent">The domain event type.</typeparam>
public abstract class DomainEventHandlerBase<TEvent> : IDomainEventHandler<TEvent>
	where TEvent : IDomainEvent
{
	/// <summary>
	/// Handles the domain event with a result.
	/// </summary>
	/// <param name="domainEvent">The domain event.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A result indicating success or failure.</returns>
	public abstract Task<Result> HandleWithResultAsync(TEvent domainEvent, CancellationToken cancellationToken);

	/// <summary>
	/// MediatR entry point. Unwraps the notification and forwards to <see cref="HandleWithResultAsync"/>.
	/// </summary>
	public async Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notification);
		await HandleWithResultAsync(notification.DomainEvent, cancellationToken);
	}
}
