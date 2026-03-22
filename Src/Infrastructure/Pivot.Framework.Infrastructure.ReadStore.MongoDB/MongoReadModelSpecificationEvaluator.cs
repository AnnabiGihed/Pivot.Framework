using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.ReadStore.MongoDB;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Applies a <see cref="ReadModelSpecification{TReadModel}"/> to an
///              <see cref="IQueryable{T}"/> obtained from <c>IMongoCollection.AsQueryable()</c>.
///
///              MongoDB's LINQ provider translates the expression tree to MongoDB aggregation
///              pipeline stages, so criteria, ordering, and paging are executed server-side.
///
///              Evaluation order: criteria → ordering → paging.
///              This is a pure static utility class — it holds no state and is thread-safe.
/// </summary>
public static class MongoReadModelSpecificationEvaluator
{
	#region Public Methods
	/// <summary>
	/// Transforms a base <see cref="IQueryable{TReadModel}"/> by applying every clause
	/// declared in the provided <paramref name="specification"/>.
	/// </summary>
	/// <typeparam name="TReadModel">The read model type being queried.</typeparam>
	/// <param name="inputQueryable">
	/// The base <see cref="IQueryable{TReadModel}"/> — typically from
	/// <c>IMongoCollection.AsQueryable()</c>. Must not be null.
	/// </param>
	/// <param name="specification">
	/// The specification that encapsulates criteria, ordering, and paging. Must not be null.
	/// </param>
	/// <returns>
	/// A new <see cref="IQueryable{TReadModel}"/> with all specification clauses applied.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="inputQueryable"/> or <paramref name="specification"/> is null.
	/// </exception>
	public static IQueryable<TReadModel> GetQuery<TReadModel>(
		IQueryable<TReadModel> inputQueryable,
		ReadModelSpecification<TReadModel> specification)
		where TReadModel : class
	{
		ArgumentNullException.ThrowIfNull(inputQueryable);
		ArgumentNullException.ThrowIfNull(specification);

		IQueryable<TReadModel> queryable = inputQueryable;

		#region Criteria
		if (specification.Criteria is not null)
			queryable = queryable.Where(specification.Criteria);
		#endregion

		#region Ordering
		if (specification.OrderByExpression is not null)
			queryable = queryable.OrderBy(specification.OrderByExpression);
		else if (specification.OrderByDescendingExpression is not null)
			queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
		#endregion

		#region Paging
		if (specification.Skip.HasValue)
			queryable = queryable.Skip(specification.Skip.Value);

		if (specification.Take.HasValue)
			queryable = queryable.Take(specification.Take.Value);
		#endregion

		return queryable;
	}
	#endregion
}
