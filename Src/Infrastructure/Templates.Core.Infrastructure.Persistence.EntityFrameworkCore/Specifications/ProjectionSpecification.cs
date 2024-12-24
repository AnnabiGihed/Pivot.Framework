using System.Linq.Expressions;
using Templates.Core.Domain.Primitives;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

public abstract class ProjectionSpecification<TProjection, TId> where TProjection : ProjectionRoot<TId>
{

	protected ProjectionSpecification(Expression<Func<TProjection, bool>>? criteria) =>
		Criteria = criteria;

	public bool IsSplitQuery { get; protected set; }

	public Expression<Func<TProjection, bool>>? Criteria { get; }

	public List<Expression<Func<TProjection, object>>> IncludeExpressions { get; } = new();

	public Expression<Func<TProjection, object>>? OrderByExpression { get; private set; }

	public Expression<Func<TProjection, object>>? OrderByDescendingExpression { get; private set; }

	protected void AddInclude(Expression<Func<TProjection, object>> includeExpression) =>
		IncludeExpressions.Add(includeExpression);

	protected void AddOrderBy(
		Expression<Func<TProjection, object>> orderByExpression) =>
		OrderByExpression = orderByExpression;

	protected void AddOrderByDescending(
		Expression<Func<TProjection, object>> orderByDescendingExpression) =>
		OrderByDescendingExpression = orderByDescendingExpression;
}