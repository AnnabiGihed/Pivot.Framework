namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines write operations for read models (projections) in a CQRS architecture.
///              Used by <see cref="ProjectionHandler{TEvent}"/> implementations to persist
///              read model state changes driven by domain events.
///
///              <see cref="UpsertAsync"/> uses insert-or-update semantics to ensure idempotency —
///              projection handlers can safely process the same event multiple times without
///              side effects (at-least-once delivery guarantee).
///
///              This interface is infrastructure-agnostic — it can be implemented by
///              EF Core, MongoDB, or any other data-access technology.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IReadModelStore<TReadModel, in TId>
	where TReadModel : class, IReadModel<TId>
{
	/// <summary>
	/// Inserts the <paramref name="readModel"/> if it does not exist, or updates it if it does.
	/// This operation is idempotent — safe to call multiple times with the same data.
	/// </summary>
	/// <param name="readModel">The read model to upsert. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	Task UpsertAsync(TReadModel readModel, CancellationToken ct = default);

	/// <summary>
	/// Deletes the read model with the given <paramref name="id"/>.
	/// No-op if the read model does not exist.
	/// </summary>
	/// <param name="id">The identifier of the read model to delete. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	Task DeleteAsync(TId id, CancellationToken ct = default);
}
