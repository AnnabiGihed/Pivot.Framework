using System.Linq.Expressions;

namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Base specification for querying read models (projections).
///              Encapsulates criteria (WHERE), ordering, and paging in a reusable,
///              composable query object.
///
///              Deliberately simpler than the write-side <c>EntitySpecification</c>:
///              - No include expressions (read models are flat / denormalised)
///              - No tracking toggle (read models are always no-tracking)
///              - No soft-delete filter (read models have no soft-delete concept)
/// </summary>
/// <typeparam name="TReadModel">The read model type to query.</typeparam>
public abstract class ReadModelSpecification<TReadModel>
	where TReadModel : class
{
	#region Fields
	private readonly List<(Expression<Func<TReadModel, object>> Expression, bool IsDescending)> _thenByExpressions = new();
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new specification with an optional criteria predicate.
	/// </summary>
	/// <param name="criteria">Optional WHERE clause. Pass <c>null</c> to match all.</param>
	protected ReadModelSpecification(Expression<Func<TReadModel, bool>>? criteria = null)
	{
		Criteria = criteria;
	}
	#endregion

	#region Properties
	/// <summary>
	/// Gets the criteria (WHERE clause) of the specification.
	/// </summary>
	public Expression<Func<TReadModel, bool>>? Criteria { get; }

	/// <summary>
	/// Gets the ascending order expression, or <c>null</c> if not set.
	/// </summary>
	public Expression<Func<TReadModel, object>>? OrderByExpression { get; private set; }

	/// <summary>
	/// Gets the descending order expression, or <c>null</c> if not set.
	/// </summary>
	public Expression<Func<TReadModel, object>>? OrderByDescendingExpression { get; private set; }

	/// <summary>
	/// Gets the secondary sort expressions applied after the primary order.
	/// </summary>
	public IReadOnlyList<(Expression<Func<TReadModel, object>> Expression, bool IsDescending)> ThenByExpressions
		=> _thenByExpressions.AsReadOnly();

	/// <summary>
	/// Gets the number of rows to skip, or <c>null</c> if paging is not applied.
	/// </summary>
	public int? Skip { get; private set; }

	/// <summary>
	/// Gets the number of rows to take, or <c>null</c> if paging is not applied.
	/// </summary>
	public int? Take { get; private set; }
	#endregion

	#region Protected Methods
	/// <summary>
	/// Enables paging for the specification.
	/// </summary>
	/// <param name="skip">Number of rows to skip (0+).</param>
	/// <param name="take">Number of rows to take (1+).</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="skip"/> is negative or <paramref name="take"/> is not positive.
	/// </exception>
	protected void ApplyPaging(int skip, int take)
	{
		if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
		if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

		Skip = skip;
		Take = take;
	}

	/// <summary>
	/// Sets an ascending ordering expression. Clears any descending order.
	/// </summary>
	/// <param name="orderByExpression">The expression to order by ascending.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="orderByExpression"/> is null.</exception>
	protected void AddOrderBy(Expression<Func<TReadModel, object>> orderByExpression)
	{
		ArgumentNullException.ThrowIfNull(orderByExpression);

		OrderByExpression = orderByExpression;
		OrderByDescendingExpression = null;
		_thenByExpressions.Clear();
	}

	/// <summary>
	/// Sets a descending ordering expression. Clears any ascending order and secondary sorts.
	/// </summary>
	/// <param name="orderByDescendingExpression">The expression to order by descending.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="orderByDescendingExpression"/> is null.</exception>
	protected void AddOrderByDescending(Expression<Func<TReadModel, object>> orderByDescendingExpression)
	{
		ArgumentNullException.ThrowIfNull(orderByDescendingExpression);

		OrderByDescendingExpression = orderByDescendingExpression;
		OrderByExpression = null;
		_thenByExpressions.Clear();
	}

	/// <summary>
	/// Adds a secondary ascending sort expression.
	/// </summary>
	/// <param name="expression">The expression to sort by ascending.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
	protected void AddThenBy(Expression<Func<TReadModel, object>> expression)
	{
		ArgumentNullException.ThrowIfNull(expression);
		_thenByExpressions.Add((expression, false));
	}

	/// <summary>
	/// Adds a secondary descending sort expression.
	/// </summary>
	/// <param name="expression">The expression to sort by descending.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
	protected void AddThenByDescending(Expression<Func<TReadModel, object>> expression)
	{
		ArgumentNullException.ThrowIfNull(expression);
		_thenByExpressions.Add((expression, true));
	}
	#endregion
}
