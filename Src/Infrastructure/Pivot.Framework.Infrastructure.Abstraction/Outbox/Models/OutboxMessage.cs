namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Represents an outbox message that stores a serialized domain event
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

	#endregion
}
