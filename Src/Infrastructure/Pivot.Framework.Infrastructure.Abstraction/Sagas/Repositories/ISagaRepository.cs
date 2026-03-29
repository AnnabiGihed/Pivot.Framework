using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Repository contract for saga instance persistence.
///              Provides CRUD operations for saga instances and their step records.
///              SaveChanges is intentionally NOT called inside repository methods.
/// </summary>
/// <typeparam name="TContext">The persistence context type, used as a DI discriminator.</typeparam>
public interface ISagaRepository<TContext> where TContext : class, IPersistenceContext
{
	#region Saga Instance Methods

	/// <summary>
	/// Retrieves a saga instance by its unique identifier.
	/// </summary>
	/// <param name="sagaId">The saga instance identifier.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>The saga instance, or null if not found.</returns>
	Task<SagaInstance?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a new saga instance to the change tracker.
	/// </summary>
	/// <param name="instance">The saga instance to persist.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> AddAsync(SagaInstance instance, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing saga instance in the change tracker.
	/// </summary>
	/// <param name="instance">The saga instance with updated state.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> UpdateAsync(SagaInstance instance, CancellationToken cancellationToken = default);

	#endregion

	#region Step Record Methods

	/// <summary>
	/// Retrieves all step records for a given saga instance, ordered by step index.
	/// </summary>
	/// <param name="sagaId">The parent saga instance identifier.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>An ordered list of step records.</returns>
	Task<IReadOnlyList<SagaStepRecord>> GetStepRecordsAsync(Guid sagaId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a new step record to the change tracker.
	/// </summary>
	/// <param name="record">The step record to persist.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> AddStepRecordAsync(SagaStepRecord record, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing step record in the change tracker.
	/// </summary>
	/// <param name="record">The step record with updated state.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> UpdateStepRecordAsync(SagaStepRecord record, CancellationToken cancellationToken = default);

	#endregion

	#region Persistence

	/// <summary>
	/// Persists all pending changes to the underlying store.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	Task SaveChangesAsync(CancellationToken cancellationToken = default);

	#endregion
}
