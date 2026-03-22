using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Applies a <see cref="ReadModelSpecification{TReadModel}"/> to an
///              <see cref="IQueryable{T}"/> using EF Core extension methods.
///
///              Deliberately simpler than <see cref="EntitySpecificationEvaluator"/>:
///              - Always applies <c>AsNoTracking</c> (read models are read-only)
///              - No include expressions (read models are flat / denormalised)
///              - No soft-delete filtering (read models have no soft-delete concept)
///              - No split-query directive (no navigation properties to split)
///
///              Evaluation order: AsNoTracking → criteria → ordering → paging.
///              This is a pure static utility class — it holds no state and is thread-safe.
/// </summary>
public static class ReadModelSpecificationEvaluator
{
	#region Public Methods
	/// <summary>
	/// Transforms a base <see cref="IQueryable{TReadModel}"/> by applying every clause
	/// declared in the provided <paramref name="specification"/>.
	/// </summary>
	/// <typeparam name="TReadModel">The read model type being queried.</typeparam>
	/// <param name="inputQueryable">
	/// The base <see cref="IQueryable{TReadModel}"/> to transform — typically
	/// <c>DbContext.Set&lt;TReadModel&gt;()</c>. Must not be null.
	/// </param>
	/// <param name="specification">
	/// The specification that encapsulates criteria, ordering, and paging. Must not be null.
	/// </param>
	/// <returns>
	/// A new <see cref="IQueryable{TReadModel}"/> with all specification clauses applied.
	/// The query is not yet materialised — it is evaluated only when enumerated.
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

		#region No-Tracking
		queryable = queryable.AsNoTracking();
		#endregion

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
