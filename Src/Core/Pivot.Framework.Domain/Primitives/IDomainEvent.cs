namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Removed <c>INotification</c> inheritance to decouple the Domain layer
///              from MediatR. The Application layer bridges domain events to MediatR via
///              <c>DomainEventNotification&lt;T&gt;</c>.
/// Purpose     : Defines the contract for a domain event.
///              A domain event represents a meaningful business occurrence within the domain model
///              and is published to notify interested handlers after state changes.
///              This interface is infrastructure-agnostic — dispatching is handled by the
///              Application and Infrastructure layers.
/// </summary>
public interface IDomainEvent
{
	/// <summary>
	/// Gets the unique identifier of the domain event instance.
	/// Used for traceability, idempotency, and event correlation.
	/// </summary>
	Guid Id { get; }

	/// <summary>
	/// Gets the UTC timestamp indicating when the domain event occurred.
	/// This value must always be expressed in UTC to ensure consistency across systems.
	/// </summary>
	DateTime OccurredOnUtc { get; }
}
