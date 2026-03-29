namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Persistence model for the event history (event store) table.
///              Represents an append-only record of a domain or integration event
///              with full envelope metadata. Written in the same transaction as business
///              state and outbox messages to guarantee consistency.
///
///              Used for:
///              - Event replay and aggregate reconstruction
///              - Audit trail and compliance
///              - Projection rebuilds
///              - Debugging and diagnostics
/// </summary>
public sealed class EventHistoryEntry
{
	/// <summary>
	/// Unique identifier of the event (same as EventEnvelope.EventId / DomainEvent.Id).
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// Fully qualified type name of the event.
	/// </summary>
	public string EventType { get; init; } = string.Empty;

	/// <summary>
	/// Schema version of this event type.
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
	/// </summary>
	public string? CausationId { get; init; }

	/// <summary>
	/// The type of the aggregate that raised this event.
	/// </summary>
	public string? AggregateType { get; init; }

	/// <summary>
	/// The unique identifier of the aggregate instance.
	/// </summary>
	public string? AggregateId { get; init; }

	/// <summary>
	/// The version of the aggregate at the time this event was raised.
	/// </summary>
	public int AggregateVersion { get; init; }

	/// <summary>
	/// Whether this event was recorded during a replay operation.
	/// </summary>
	public bool ReplayFlag { get; init; }

	/// <summary>
	/// The serialized event payload (JSON). Stored as JSONB on PostgreSQL, nvarchar(max) on SQL Server.
	/// </summary>
	public string Payload { get; init; } = string.Empty;

	/// <summary>
	/// The UTC timestamp when this record was persisted to the event store.
	/// May differ from OccurredOnUtc if there's processing delay.
	/// </summary>
	public DateTime CreatedAtUtc { get; init; }
}
