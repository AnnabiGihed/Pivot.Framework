namespace Pivot.Framework.Domain.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Represents the lifecycle status of an individual saga step.
/// </summary>
public enum SagaStepStatus
{
	/// <summary>The step is queued but has not started.</summary>
	Pending = 0,

	/// <summary>The step is currently executing.</summary>
	Executing = 1,

	/// <summary>The step completed successfully.</summary>
	Completed = 2,

	/// <summary>The step is being compensated (rolled back).</summary>
	Compensating = 3,

	/// <summary>The step compensation completed successfully.</summary>
	Compensated = 4,

	/// <summary>The step failed and could not be compensated.</summary>
	Failed = 5
}
