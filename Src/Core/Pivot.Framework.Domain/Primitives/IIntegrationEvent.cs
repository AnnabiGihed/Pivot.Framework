namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines the contract for an integration event.
///              An integration event represents a cross-service boundary occurrence that
///              is published to external consumers via the message broker.
///              Unlike <see cref="IDomainEvent"/>, integration events carry a
///              <see cref="CorrelationId"/> for end-to-end distributed tracing and are
///              always routed to the external broker (never dispatched in-process only).
///
///              This interface is intentionally separate from <see cref="IDomainEvent"/>
///              to enforce the distinction between internal domain semantics and
///              external contract events.
/// </summary>
public interface IIntegrationEvent
{
	/// <summary>
	/// Gets the unique identifier of the integration event instance.
	/// Used for idempotency and deduplication at the consumer side.
	/// </summary>
	Guid Id { get; }

	/// <summary>
	/// Gets the UTC timestamp indicating when the integration event occurred.
	/// </summary>
	DateTime OccurredOnUtc { get; }

	/// <summary>
	/// Gets the correlation identifier for distributed tracing across services.
	/// May be null if no correlation context was available when the event was created.
	/// </summary>
	string? CorrelationId { get; }
}
