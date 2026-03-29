namespace Pivot.Framework.Domain.IntegrationEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Integration event emitted when an outbox message permanently fails
///              after exceeding the configured maximum retry count.
///              Downstream consumers can use this event to trigger compensation flows
///              (e.g., refund a payment, cancel an order intent, notify operations).
/// </summary>
public sealed record OutboxMessageFailedEvent : IntegrationEvent
{
	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="OutboxMessageFailedEvent"/>.
	/// </summary>
	/// <param name="failedMessageId">The identifier of the outbox message that failed.</param>
	/// <param name="eventType">The fully qualified type name of the original event.</param>
	/// <param name="retryCount">The number of retry attempts before failure.</param>
	/// <param name="lastError">The error message from the last failed attempt.</param>
	/// <param name="correlationId">The correlation identifier for tracing.</param>
	public OutboxMessageFailedEvent(
		Guid failedMessageId,
		string eventType,
		int retryCount,
		string? lastError,
		string? correlationId)
		: base(Guid.NewGuid(), DateTime.UtcNow, correlationId)
	{
		FailedMessageId = failedMessageId;
		OriginalEventType = eventType;
		RetryCount = retryCount;
		LastError = lastError;
	}

	/// <summary>
	/// Parameterless constructor for deserialization.
	/// </summary>
	public OutboxMessageFailedEvent() : base() { }

	#endregion

	#region Properties

	/// <summary>
	/// The identifier of the outbox message that permanently failed.
	/// </summary>
	public Guid FailedMessageId { get; init; }

	/// <summary>
	/// The fully qualified type name of the original event that could not be published.
	/// </summary>
	public string OriginalEventType { get; init; } = string.Empty;

	/// <summary>
	/// The total number of retry attempts before the message was dead-lettered.
	/// </summary>
	public int RetryCount { get; init; }

	/// <summary>
	/// The error message from the last failed processing attempt.
	/// </summary>
	public string? LastError { get; init; }

	#endregion
}
