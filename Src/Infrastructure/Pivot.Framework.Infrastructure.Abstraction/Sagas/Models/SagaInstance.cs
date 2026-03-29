using Pivot.Framework.Domain.Sagas;

namespace Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Persistence model for a saga instance.
///              Tracks the lifecycle state, current step position, serialized saga data,
///              and timing information for a running or completed saga.
///              Uses optimistic concurrency via <see cref="Version"/> to prevent
///              concurrent modifications from multiple processors.
/// </summary>
public sealed class SagaInstance
{
	#region Properties

	/// <summary>
	/// The unique identifier of the saga instance.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// The type name of the saga (e.g., "OrderPayment").
	/// Used for querying and grouping saga instances.
	/// </summary>
	public string SagaType { get; init; } = string.Empty;

	/// <summary>
	/// The current lifecycle state of the saga.
	/// </summary>
	public SagaState State { get; set; } = SagaState.NotStarted;

	/// <summary>
	/// The zero-based index of the current step being executed or compensated.
	/// </summary>
	public int CurrentStepIndex { get; set; }

	/// <summary>
	/// The JSON-serialized saga data that carries state across steps.
	/// </summary>
	public string? SerializedData { get; set; }

	/// <summary>
	/// The correlation identifier for end-to-end distributed tracing.
	/// </summary>
	public string? CorrelationId { get; init; }

	/// <summary>
	/// The UTC timestamp when the saga was started.
	/// </summary>
	public DateTime StartedAtUtc { get; init; }

	/// <summary>
	/// The UTC timestamp when the saga completed, was compensated, or failed.
	/// Null while the saga is in progress.
	/// </summary>
	public DateTime? CompletedAtUtc { get; set; }

	/// <summary>
	/// The reason for saga failure, if applicable.
	/// </summary>
	public string? FailureReason { get; set; }

	/// <summary>
	/// Optimistic concurrency token. Incremented on every state transition
	/// to detect conflicting updates from concurrent processors.
	/// </summary>
	public int Version { get; set; }

	#endregion
}
