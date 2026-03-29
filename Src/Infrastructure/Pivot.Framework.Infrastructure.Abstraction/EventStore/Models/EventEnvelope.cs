namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Represents the full metadata envelope for an event in the event store.
///              Contains the 12 mandatory fields required for enterprise-grade event sourcing,
///              audit trails, and cross-service distributed tracing.
///
///              This envelope is built by the infrastructure during event persistence and
///              is used to populate both the event history table and the outbox message.
/// </summary>
public sealed class EventEnvelope
{
	/// <summary>
	/// Unique identifier of the event instance.
	/// </summary>
	public Guid EventId { get; init; }

	/// <summary>
	/// Fully qualified type name of the event (assembly-qualified for deserialization).
	/// </summary>
	public string EventType { get; init; } = string.Empty;

	/// <summary>
	/// Schema version of this event type. Enables event versioning and migration.
	/// Defaults to 1 for new events.
	/// </summary>
	public int EventVersion { get; init; } = 1;

	/// <summary>
	/// UTC timestamp when the event occurred in the domain.
	/// </summary>
	public DateTime OccurredOnUtc { get; init; }

	/// <summary>
	/// Identifier of the service/microservice that produced this event.
	/// </summary>
	public string ProducerService { get; init; } = string.Empty;

	/// <summary>
	/// End-to-end correlation identifier for distributed tracing.
	/// </summary>
	public string? CorrelationId { get; init; }

	/// <summary>
	/// The EventId of the event that caused this event to be raised.
	/// Null if the event originates from a user/external action.
	/// </summary>
	public string? CausationId { get; init; }

	/// <summary>
	/// The type of the aggregate that raised this event (e.g., "Order", "Payment").
	/// </summary>
	public string? AggregateType { get; init; }

	/// <summary>
	/// The unique identifier of the aggregate instance that raised this event.
	/// </summary>
	public string? AggregateId { get; init; }

	/// <summary>
	/// The version of the aggregate at the time this event was raised.
	/// Used for optimistic concurrency and event ordering.
	/// </summary>
	public int AggregateVersion { get; init; }

	/// <summary>
	/// Indicates whether this event is being replayed (projection rebuild).
	/// When true, handlers should suppress non-projection side effects.
	/// </summary>
	public bool ReplayFlag { get; init; }

	/// <summary>
	/// The serialized event payload (JSON).
	/// </summary>
	public string Payload { get; init; } = string.Empty;
}
