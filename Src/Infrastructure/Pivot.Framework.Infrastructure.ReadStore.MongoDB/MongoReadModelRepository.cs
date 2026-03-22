using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : MongoDB implementation of <see cref="IReadModelRepository{TReadModel,TId}"/>.
///              Uses <see cref="IMongoCollection{TReadModel}"/> for all queries.
///              Collection name is derived from the read model type name.
///
///              Designed for inheritance — concrete repositories may override any virtual method
///              to add custom query behaviour using native MongoDB operations.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class MongoReadModelRepository<TReadModel, TId> : IReadModelRepository<TReadModel, TId>
	where TReadModel : class, IReadModel<TId>
{
	#region Fields
	/// <summary>
	/// The MongoDB collection backing this repository.
	/// Exposed as <c>protected</c> so that derived repositories can access it for custom queries.
	/// </summary>
	protected readonly IMongoCollection<TReadModel> Collection;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="MongoReadModelRepository{TReadModel,TId}"/>.
	/// The collection name is derived from <typeparamref name="TReadModel"/>'s type name.
	/// </summary>
	/// <param name="database">The MongoDB database. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="database"/> is null.</exception>
	public MongoReadModelRepository(IMongoDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		Collection = MongoCollectionResolver.GetCollection<TReadModel>(database);
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public virtual async Task<TReadModel?> GetByIdAsync(TId id, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(id);

		var filter = Builders<TReadModel>.Filter.Eq(x => x.Id, id);
		return await Collection.Find(filter).FirstOrDefaultAsync(ct);
	}

	/// <inheritdoc />
	public virtual async Task<IReadOnlyList<TReadModel>> GetAllAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		if (predicate is null)
			return await Collection.Find(Builders<TReadModel>.Filter.Empty).ToListAsync(ct);

		return await Collection.Find(predicate).ToListAsync(ct);
	}

	/// <inheritdoc />
	public virtual async Task<bool> ExistsAsync(
		Expression<Func<TReadModel, bool>> predicate,
		CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(predicate);

		return await Collection.Find(predicate).AnyAsync(ct);
	}

	/// <inheritdoc />
	public virtual async Task<int> CountAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		if (predicate is null)
		{
			var estimatedCount = await Collection.EstimatedDocumentCountAsync(cancellationToken: ct);
			return estimatedCount > int.MaxValue ? int.MaxValue : (int)estimatedCount;
		}

		var count = await Collection.CountDocumentsAsync(
			new ExpressionFilterDefinition<TReadModel>(predicate), cancellationToken: ct);
		return count > int.MaxValue ? int.MaxValue : (int)count;
	}

	/// <inheritdoc />
	public virtual async Task<IReadOnlyList<TReadModel>> ListAsync(
		ReadModelSpecification<TReadModel> specification,
		CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(specification);

		var queryable = MongoReadModelSpecificationEvaluator.GetQuery(
			Collection.AsQueryable(), specification);

		return await queryable.ToListAsync(ct);
	}
	#endregion
}
