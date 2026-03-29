using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Abstractions.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines a single step within a saga orchestration.
///              Each step provides both a forward execution path and a compensation
///              (rollback) path. The saga orchestrator calls <see cref="ExecuteAsync"/>
///              during the happy path and <see cref="CompensateAsync"/> during rollback.
///
///              Implementations must be idempotent — the orchestrator may retry a step
///              or its compensation if the previous attempt was interrupted.
/// </summary>
/// <typeparam name="TSagaData">
/// The saga-specific data type that carries state across steps.
/// Must be serializable to JSON for persistence.
/// </typeparam>
public interface ISagaStep<TSagaData> where TSagaData : class
{
	/// <summary>
	/// Gets the unique name of this step within the saga.
	/// Used for logging, tracking, and step identification in the saga repository.
	/// </summary>
	string StepName { get; }

	/// <summary>
	/// Executes the forward action for this step.
	/// </summary>
	/// <param name="data">The saga data carrying state from previous steps.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> ExecuteAsync(TSagaData data, CancellationToken cancellationToken);

	/// <summary>
	/// Executes the compensation (rollback) action for this step.
	/// Called when a later step fails and the saga needs to undo completed work.
	/// </summary>
	/// <param name="data">The saga data carrying state from previous steps.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of compensation.</returns>
	Task<Result> CompensateAsync(TSagaData data, CancellationToken cancellationToken);
}
