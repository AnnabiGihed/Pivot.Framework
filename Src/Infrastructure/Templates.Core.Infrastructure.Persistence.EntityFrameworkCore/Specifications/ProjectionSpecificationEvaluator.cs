using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

public static class ProjectionSpecificationEvaluator<TId>
{
	public static IQueryable<TProjection> GetQuery<TProjection>(
		IQueryable<TProjection> inputQueryable,
		ProjectionSpecification<TProjection, TId> specification)
		where TProjection : ProjectionRoot<TId>
	{
		IQueryable<TProjection> queryable = inputQueryable;

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