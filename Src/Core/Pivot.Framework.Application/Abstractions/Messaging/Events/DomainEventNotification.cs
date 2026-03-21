using MediatR;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : MediatR notification adapter that wraps a domain event.
///              This bridges the Domain layer (which has zero MediatR dependency) to the
///              Application layer's MediatR-based in-process dispatching pipeline.
///
///              The Domain layer defines <see cref="IDomainEvent"/> as a pure contract.
///              The Application layer owns the MediatR integration boundary via this adapter,
///              preserving Clean Architecture's dependency rule — outer layers depend inward,
///              never the reverse.
///
///              Infrastructure dispatchers wrap domain events in this type before publishing
///              through MediatR. Handlers receive the wrapper and access the inner event via
///              <see cref="DomainEvent"/>.
/// </summary>
/// <typeparam name="TDomainEvent">The concrete domain event type.</typeparam>
public sealed class DomainEventNotification<TDomainEvent> : INotification
	where TDomainEvent : IDomainEvent
{
	#region Properties
	/// <summary>
	/// Gets the domain event carried by this notification.
	/// </summary>
	public TDomainEvent DomainEvent { get; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new notification wrapping the specified domain event.
	/// </summary>
	/// <param name="domainEvent">The domain event to wrap. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
	public DomainEventNotification(TDomainEvent domainEvent)
	{
		DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
	}
	#endregion
}
