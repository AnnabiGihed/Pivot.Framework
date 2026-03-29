using Pivot.Framework.Domain.Sagas;

namespace Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Persistence model for tracking the execution status of an individual saga step.
///              Each step record is linked to a <see cref="SagaInstance"/> and provides
///              an audit trail of step execution, timing, and error information.
/// </summary>
public sealed class SagaStepRecord
{
	#region Properties

	/// <summary>
	/// The unique identifier of the step record.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// The identifier of the parent saga instance.
	/// </summary>
	public Guid SagaInstanceId { get; init; }

	/// <summary>
	/// The name of the step (from <see cref="ISagaStep{TSagaData}.StepName"/>).
	/// </summary>
	public string StepName { get; init; } = string.Empty;

	/// <summary>
	/// The zero-based index of the step within the saga definition.
	/// </summary>
	public int StepIndex { get; init; }

	/// <summary>
	/// The current execution status of the step.
	/// </summary>
	public SagaStepStatus Status { get; set; } = SagaStepStatus.Pending;

	/// <summary>
	/// The UTC timestamp when the step started executing.
	/// </summary>
	public DateTime? StartedAtUtc { get; set; }

	/// <summary>
	/// The UTC timestamp when the step completed (success, failure, or compensation).
	/// </summary>
	public DateTime? CompletedAtUtc { get; set; }

	/// <summary>
	/// The error message if the step failed, or null on success.
	/// </summary>
	public string? Error { get; set; }

	#endregion
}
