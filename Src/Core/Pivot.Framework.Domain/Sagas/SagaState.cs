namespace Pivot.Framework.Domain.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Represents the lifecycle state of a saga instance.
/// </summary>
public enum SagaState
{
	/// <summary>The saga has been created but no steps have been executed.</summary>
	NotStarted = 0,

	/// <summary>The saga is actively executing steps.</summary>
	InProgress = 1,

	/// <summary>All steps completed successfully.</summary>
	Completed = 2,

	/// <summary>A step has failed and compensation is in progress.</summary>
	Compensating = 3,

	/// <summary>All compensation steps have completed successfully.</summary>
	Compensated = 4,

	/// <summary>The saga failed and compensation could not complete.</summary>
	Failed = 5
}
