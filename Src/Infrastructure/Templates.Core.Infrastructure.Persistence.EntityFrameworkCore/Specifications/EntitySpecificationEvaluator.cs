using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

public static class EntitySpecificationEvaluator<TId>
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="inputQueryable"></param>
	/// <param name="specification"></param>
	/// <returns></returns>
	public static IQueryable<TEntity> GetQuery<TEntity, TId>(
		IQueryable<TEntity> inputQueryable,
		EntitySpecification<TEntity, TId> specification)
		where TEntity : Entity<TId>
		where TId : IComparable<TId>
	{
		IQueryable<TEntity> queryable = inputQueryable;

		if (specification.Criteria is not null)
		{
			queryable = queryable.Where(specification.Criteria);
		}

		queryable = specification.IncludeExpressions.Aggregate(
			queryable,
			(current, includeExpression) =>
				current.Include(includeExpression));

		if (specification.OrderByExpression is not null)
		{
			queryable = queryable.OrderBy(specification.OrderByExpression);
		}
		else if (specification.OrderByDescendingExpression is not null)
		{
			queryable = queryable.OrderByDescending(
				specification.OrderByDescendingExpression);
		}

		if (specification.IsSplitQuery)
		{
			queryable = queryable.AsSplitQuery();
		}

		return queryable;
	}
}
