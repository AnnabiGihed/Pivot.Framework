using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Repositories;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core base implementation of <see cref="IAsyncQueryRepository{TProjection,TId}"/>.
///              All queries run with <c>AsNoTracking</c> for optimal read performance.
///              Designed for CQRS read-side repositories backed by projection / read-model tables.
///              Designed for inheritance — concrete repositories may override any virtual method to
///              add domain-specific behaviour.
/// </summary>
/// <typeparam name="TProjection">The projection / read-model type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type of the projection.</typeparam>
public class BaseAsyncQueryRepository<TProjection, TId> : IAsyncQueryRepository<TProjection, TId>
	where TProjection : ProjectionRoot<TId>
	where TId : IStronglyTypedId<TId>
{
	#region Fields
	/// <summary>
	/// The EF Core <see cref="DbContext"/> used by this repository.
	/// Exposed as <c>protected</c> so that derived repositories can access it for custom queries.
	/// </summary>
	protected readonly DbContext DbContext;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="BaseAsyncQueryRepository{TProjection,TId}"/> with the provided
	/// <see cref="DbContext"/>.
	/// </summary>
	/// <param name="dbContext">The EF Core database context. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
	public BaseAsyncQueryRepository(DbContext dbContext)
	{
		DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Returns the <typeparamref name="TProjection"/> whose primary key equals <paramref name="id"/>,
	/// or <c>null</c> when no match is found.
	/// The entity is NOT change-tracked (read-only via <c>AsNoTracking</c>).
	/// </summary>
	/// <param name="id">The strongly-typed identifier to look up. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	/// <returns>The matching projection, or <c>null</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
	public virtual async Task<TProjection?> GetByIdAsync(TId id, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(id);

		return await DbContext.Set<TProjection>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id!.Equals(id), ct);
	}

	/// <summary>
	/// Returns all <typeparamref name="TProjection"/> instances matching the given <paramref name="predicate"/>.
	/// When <paramref name="predicate"/> is <c>null</c>, returns all projections.
	/// Results are NOT change-tracked (read-only via <c>AsNoTracking</c>).
	/// </summary>
	/// <param name="predicate">Optional LINQ predicate to evaluate server-side. Pass <c>null</c> to retrieve all.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	/// <returns>A read-only list of matching projections.</returns>
	public virtual async Task<IReadOnlyList<TProjection>> GetAllAsync(
		Expression<Func<TProjection, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		var query = DbContext.Set<TProjection>().AsNoTracking();

		if (predicate is not null)
			query = query.Where(predicate);

		return await query.ToListAsync(ct);
	}

	/// <summary>
	/// Returns <c>true</c> when at least one <typeparamref name="TProjection"/> in the store satisfies
	/// <paramref name="predicate"/>; <c>false</c> otherwise.
	/// Runs as a lightweight server-side <c>EXISTS</c> check — no entity is materialised.
	/// </summary>
	/// <param name="predicate">The LINQ predicate to evaluate server-side. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	/// <returns><c>true</c> if a matching projection exists; otherwise <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
	public virtual Task<bool> ExistsAsync(Expression<Func<TProjection, bool>> predicate, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(predicate);

		return DbContext.Set<TProjection>()
			.AsNoTracking()
			.AnyAsync(predicate, ct);
	}

	/// <summary>
	/// Returns the count of <typeparamref name="TProjection"/> instances matching the given <paramref name="predicate"/>.
	/// When <paramref name="predicate"/> is <c>null</c>, returns the total count.
	/// </summary>
	/// <param name="predicate">Optional LINQ predicate to evaluate server-side. Pass <c>null</c> to count all.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	/// <returns>The number of matching projections.</returns>
	public virtual async Task<int> CountAsync(
		Expression<Func<TProjection, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		var query = DbContext.Set<TProjection>().AsNoTracking();

		if (predicate is not null)
			query = query.Where(predicate);

		return await query.CountAsync(ct);
	}
	#endregion
}
