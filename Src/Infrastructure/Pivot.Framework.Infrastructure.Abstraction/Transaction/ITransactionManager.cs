using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Transaction;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for managing persistence transactions.
///              Designed to be used by middleware to own the transaction boundary.
/// </summary>
public interface ITransactionManager<TContext> where TContext : class, IPersistenceContext
{
	#region Methods

	/// <summary>
	/// Begins a new database transaction asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	Task BeginTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Commits the current database transaction asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Rolls back the current database transaction asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

	#endregion
}
