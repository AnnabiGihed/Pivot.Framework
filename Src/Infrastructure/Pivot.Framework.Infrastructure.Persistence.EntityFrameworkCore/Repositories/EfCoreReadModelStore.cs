using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IReadModelStore{TReadModel,TId}"/>.
///              Used by projection handlers to persist read model state changes.
///
///              <see cref="UpsertAsync"/> uses a find-then-add-or-update pattern with retry
///              on duplicate key to ensure idempotency under concurrent access.
///              This is critical for fintech reliability — projection handlers may process
///              the same event multiple times (at-least-once delivery) and must produce
///              the same result.
///
///              Designed for inheritance — concrete stores may override any virtual method.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class EfCoreReadModelStore<TReadModel, TId> : IReadModelStore<TReadModel, TId>
	where TReadModel : class, IReadModel<TId>
{
	#region Fields
	/// <summary>
	/// The EF Core <see cref="DbContext"/> used by this store.
	/// Exposed as <c>protected</c> so that derived stores can access it.
	/// </summary>
	protected readonly DbContext DbContext;

	private readonly ILogger<EfCoreReadModelStore<TReadModel, TId>> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="EfCoreReadModelStore{TReadModel,TId}"/>.
	/// </summary>
	/// <param name="dbContext">The EF Core database context. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	public EfCoreReadModelStore(
		DbContext dbContext,
		ILogger<EfCoreReadModelStore<TReadModel, TId>> logger)
	{
		DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Inserts the <paramref name="readModel"/> if it does not exist, or updates it if it does.
	/// Uses a find-then-add-or-update pattern with retry on duplicate key for idempotency
	/// under concurrent access.
	/// </summary>
	/// <param name="readModel">The read model to upsert. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	public virtual async Task UpsertAsync(TReadModel readModel, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(readModel);

		try
		{
			var existing = await DbContext.Set<TReadModel>()
				.FindAsync(new object[] { readModel.Id! }, ct);

			if (existing is not null)
			{
				DbContext.Entry(existing).CurrentValues.SetValues(readModel);
			}
			else
			{
				await DbContext.Set<TReadModel>().AddAsync(readModel, ct);
			}

			await DbContext.SaveChangesAsync(ct);
		}
		catch (DbUpdateException) when (!ct.IsCancellationRequested)
		{
			// Concurrent insert race: another thread inserted between FindAsync and AddAsync.
			// Detach the failed entry and retry as an update.
			_logger.LogWarning(
				"Concurrent insert detected for {ReadModelType} with Id {Id}. Retrying as update.",
				typeof(TReadModel).Name, readModel.Id);

			DetachAll();

			var existing = await DbContext.Set<TReadModel>()
				.FindAsync(new object[] { readModel.Id! }, ct);

			if (existing is not null)
			{
				DbContext.Entry(existing).CurrentValues.SetValues(readModel);
				await DbContext.SaveChangesAsync(ct);
			}
		}
	}

	/// <summary>
	/// Deletes the read model with the given <paramref name="id"/>.
	/// No-op if the read model does not exist.
	/// </summary>
	/// <param name="id">The identifier. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	public virtual async Task DeleteAsync(TId id, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(id);

		var existing = await DbContext.Set<TReadModel>()
			.FindAsync(new object[] { id }, ct);

		if (existing is not null)
		{
			DbContext.Set<TReadModel>().Remove(existing);
			await DbContext.SaveChangesAsync(ct);
		}
	}
	#endregion

	#region Private Methods
	/// <summary>
	/// Detaches all tracked entities to reset the change tracker after a failed operation.
	/// </summary>
	private void DetachAll()
	{
		foreach (var entry in DbContext.ChangeTracker.Entries().ToList())
		{
			entry.State = EntityState.Detached;
		}
	}
	#endregion
}
