namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Discriminates between domain events (internal to the bounded context)
///              and integration events (cross-service boundary).
///              Used by the outbox processor to route messages to the appropriate publisher.
/// </summary>
public enum MessageKind
{
	/// <summary>
	/// A domain event that is meaningful within the bounded context.
	/// May be dispatched in-process or via the broker depending on configuration.
	/// </summary>
	DomainEvent = 0,

	/// <summary>
	/// An integration event intended for cross-service communication.
	/// Always published to the external message broker.
	/// </summary>
	IntegrationEvent = 1
}
