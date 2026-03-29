using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Abstractions.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Orchestrates saga execution: starting new sagas, advancing steps on success,
///              and triggering compensation on failure.
///
///              The orchestrator persists saga state after each step transition, ensuring
///              that the saga can be resumed if the process crashes mid-execution.
///              Compensation walks backward through completed steps, calling
///              <see cref="ISagaStep{TSagaData}.CompensateAsync"/> on each.
/// </summary>
public interface ISagaOrchestrator
{
	/// <summary>
	/// Starts a new saga instance and begins executing steps from the first one.
	/// </summary>
	/// <typeparam name="TSagaData">The saga-specific data type.</typeparam>
	/// <param name="definition">The saga definition containing the ordered list of steps.</param>
	/// <param name="data">The initial saga data.</param>
	/// <param name="correlationId">Optional correlation ID for distributed tracing.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result{Guid}"/> containing the saga instance ID on success.</returns>
	Task<Result<Guid>> StartAsync<TSagaData>(
		ISagaDefinition<TSagaData> definition,
		TSagaData data,
		string? correlationId = null,
		CancellationToken cancellationToken = default)
		where TSagaData : class;

	/// <summary>
	/// Resumes a previously started saga from its current step.
	/// Used for recovering sagas that were interrupted (e.g., process restart).
	/// </summary>
	/// <typeparam name="TSagaData">The saga-specific data type.</typeparam>
	/// <param name="sagaId">The identifier of the saga instance to resume.</param>
	/// <param name="definition">The saga definition containing the ordered list of steps.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> ResumeAsync<TSagaData>(
		Guid sagaId,
		ISagaDefinition<TSagaData> definition,
		CancellationToken cancellationToken = default)
		where TSagaData : class;
}
