using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Maps a domain event to zero or more integration events that should be
///              published to the outbox in the same transaction.
///              Keeps domain-to-contract translation explicit, reusable, and testable.
/// </summary>
/// <typeparam name="TDomainEvent">The domain event type handled by this mapper.</typeparam>
public interface IIntegrationEventMapper<in TDomainEvent>
	where TDomainEvent : IDomainEvent
{
	/// <summary>
	/// Maps the supplied domain event to zero or more integration events.
	/// </summary>
	/// <param name="domainEvent">The domain event to translate.</param>
	/// <returns>The integration events to enqueue. May be empty, but must not be null.</returns>
	IEnumerable<IIntegrationEvent> Map(TDomainEvent domainEvent);
}
