using System.Linq.Expressions;
using CSharpFunctionalExtensions;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

public abstract class EntitySpecification<TEntity, TId> where TEntity : Entity<TId> where TId : IComparable<TId>
{
	//TODO: distinguish from entity specification pattern and projection specification pattern

	protected EntitySpecification(Expression<Func<TEntity, bool>>? criteria) =>
		Criteria = criteria;

	public bool IsSplitQuery { get; protected set; }

	public Expression<Func<TEntity, bool>>? Criteria { get; }

	public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();

	public Expression<Func<TEntity, object>>? OrderByExpression { get; private set; }

	public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; private set; }

	protected void AddInclude(Expression<Func<TEntity, object>> includeExpression) =>
		IncludeExpressions.Add(includeExpression);

	protected void AddOrderBy(
		Expression<Func<TEntity, object>> orderByExpression) =>
		OrderByExpression = orderByExpression;

	protected void AddOrderByDescending(
		Expression<Func<TEntity, object>> orderByDescendingExpression) =>
		OrderByDescendingExpression = orderByDescendingExpression;
}
