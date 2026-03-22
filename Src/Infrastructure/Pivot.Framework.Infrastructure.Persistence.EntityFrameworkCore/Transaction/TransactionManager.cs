using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Transaction;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : EF Core transaction manager.
///              - Idempotent (won't start a new transaction if one already exists)
///              - Commits/Rolls back only when a transaction exists
///              - Disposes the transaction after completion to avoid leaks
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type scoped to this transaction manager.</typeparam>
public sealed class TransactionManager<TContext> : ITransactionManager<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Fields
	/// <summary>
	/// The EF Core database context whose transactions are managed.
	/// </summary>
	private readonly TContext _dbContext;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="TransactionManager{TContext}"/> with the provided DbContext.
	/// </summary>
	/// <param name="dbContext">The EF Core database context. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
	public TransactionManager(TContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Begins a new database transaction if one is not already active.
	/// Idempotent — calling this when a transaction already exists is a no-op.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		// Avoid nested/duplicate transactions
		if (_dbContext.Database.CurrentTransaction is not null)
			return;

		await _dbContext.Database.BeginTransactionAsync(cancellationToken);
	}

	/// <summary>
	/// Commits the current database transaction if one is active.
	/// No-op when no transaction exists.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_dbContext.Database.CurrentTransaction is null)
			return;

		await _dbContext.Database.CommitTransactionAsync(cancellationToken);
	}

	/// <summary>
	/// Rolls back the current database transaction if one is active.
	/// No-op when no transaction exists.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_dbContext.Database.CurrentTransaction is null)
			return;

		await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
	}
	#endregion
}
