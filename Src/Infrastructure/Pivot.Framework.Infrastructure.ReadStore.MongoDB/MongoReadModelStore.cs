using MongoDB.Driver;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : MongoDB implementation of <see cref="IReadModelStore{TReadModel,TId}"/>.
///              Uses native MongoDB <c>ReplaceOneAsync</c> with <c>IsUpsert = true</c>
///              for idempotent insert-or-update semantics — no race conditions, no retries needed.
///
///              This is inherently safer than the EF Core pattern because MongoDB's
///              upsert is a single atomic server-side operation.
///
///              Collection name is derived from the read model type name.
///              Designed for inheritance — concrete stores may override any virtual method.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class MongoReadModelStore<TReadModel, TId> : IReadModelStore<TReadModel, TId>
	where TReadModel : class, IReadModel<TId>
{
	#region Fields
	/// <summary>
	/// The MongoDB collection backing this store.
	/// Exposed as <c>protected</c> so that derived stores can access it.
	/// </summary>
	protected readonly IMongoCollection<TReadModel> Collection;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="MongoReadModelStore{TReadModel,TId}"/>.
	/// </summary>
	/// <param name="database">The MongoDB database. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="database"/> is null.</exception>
	public MongoReadModelStore(IMongoDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		Collection = MongoCollectionResolver.GetCollection<TReadModel>(database);
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Inserts the <paramref name="readModel"/> if it does not exist, or replaces it if it does.
	/// Uses MongoDB's native upsert — a single atomic server-side operation.
	/// Idempotent by design.
	/// </summary>
	/// <param name="readModel">The read model to upsert. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	public virtual async Task UpsertAsync(TReadModel readModel, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(readModel);

		var filter = Builders<TReadModel>.Filter.Eq(x => x.Id, readModel.Id);

		await Collection.ReplaceOneAsync(
			filter,
			readModel,
			new ReplaceOptions { IsUpsert = true },
			ct);
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

		var filter = Builders<TReadModel>.Filter.Eq(x => x.Id, id);
		await Collection.DeleteOneAsync(filter, ct);
	}
	#endregion
}
