namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Added <see cref="CorrelationId"/> for end-to-end tracing,
///              <see cref="Kind"/> to discriminate domain vs integration events,
///              <see cref="FailedAtUtc"/> and <see cref="LastError"/> for dead-letter tracking.
/// Purpose     : Represents an outbox message that stores a serialized domain or integration event
///              for reliable asynchronous processing and delivery.
/// </summary>
public sealed class OutboxMessage
{
	#region Properties

	/// <summary>
	/// The unique identifier of the outbox message.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// The serialized payload of the domain event.
	/// </summary>
	public string? Payload { get; init; }

	/// <summary>
	/// The fully qualified type name of the domain event.
	/// </summary>
	public string? EventType { get; init; }

	/// <summary>
	/// The UTC date and time when the outbox message was created.
	/// </summary>
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The number of times processing has been retried for this message.
	/// </summary>
	public int RetryCount { get; set; } = 0;

	/// <summary>
	/// Indicates whether the message has been successfully processed.
	/// </summary>
	public bool Processed { get; set; } = false;

	/// <summary>
	/// The UTC date and time when the message was processed, or null if not yet processed.
	/// </summary>
	public DateTime? ProcessedAtUtc { get; set; }

	/// <summary>
	/// The correlation identifier for end-to-end distributed tracing.
	/// Propagated from the originating request through the outbox to downstream consumers.
	/// </summary>
	public string? CorrelationId { get; init; }

	/// <summary>
	/// Discriminates between domain events (internal) and integration events (cross-service).
	/// Defaults to <see cref="MessageKind.DomainEvent"/> for backward compatibility.
	/// </summary>
	public MessageKind Kind { get; init; } = MessageKind.DomainEvent;

	/// <summary>
	/// The UTC date and time when the message was permanently failed after exceeding
	/// the maximum retry count. Null if the message has not been dead-lettered.
	/// </summary>
	public DateTime? FailedAtUtc { get; set; }

	/// <summary>
	/// The error message from the last failed processing attempt.
	/// Used for diagnostics and dead-letter monitoring.
	/// </summary>
	public string? LastError { get; set; }

	#endregion
}
