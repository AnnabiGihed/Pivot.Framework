namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration options for outbox message retry and dead-letter behaviour.
///              When a message exceeds <see cref="MaxRetryCount"/>, it is permanently marked
///              as failed and optionally an <c>OutboxMessageFailedEvent</c> is emitted to
///              the outbox for downstream consumers to handle (e.g., trigger compensation).
/// </summary>
public sealed class OutboxRetryOptions
{
	#region Properties

	/// <summary>
	/// The maximum number of times a failed outbox message will be retried before
	/// being dead-lettered. Defaults to 5.
	/// </summary>
	public int MaxRetryCount { get; set; } = 5;

	/// <summary>
	/// When true, an <c>OutboxMessageFailedEvent</c> integration event is emitted
	/// to the outbox when a message exceeds <see cref="MaxRetryCount"/>.
	/// This allows downstream services to trigger compensation flows.
	/// Defaults to true.
	/// </summary>
	public bool EmitFailureEvent { get; set; } = true;

	#endregion
}
