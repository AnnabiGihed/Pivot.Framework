using System.Linq.Expressions;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines read-only query operations for projection roots (read models)
///              in a CQRS architecture. Query repositories never modify state and use
///              no-tracking queries for optimal performance.
/// </summary>
/// <typeparam name="TProjection">The projection / read-model type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IAsyncQueryRepository<TProjection, TId>
	where TProjection : ProjectionRoot<TId>
	where TId : IStronglyTypedId<TId>
{
	/// <summary>
	/// Returns the <typeparamref name="TProjection"/> with the given <paramref name="id"/>,
	/// or <c>null</c> when no match is found.
	/// The entity is NOT change-tracked (read-only).
	/// </summary>
	Task<TProjection?> GetByIdAsync(TId id, CancellationToken ct = default);

	/// <summary>
	/// Returns all <typeparamref name="TProjection"/> instances matching the given <paramref name="predicate"/>.
	/// Results are NOT change-tracked (read-only).
	/// </summary>
	Task<IReadOnlyList<TProjection>> GetAllAsync(Expression<Func<TProjection, bool>>? predicate = null, CancellationToken ct = default);

	/// <summary>
	/// Returns <c>true</c> when at least one <typeparamref name="TProjection"/> satisfies
	/// <paramref name="predicate"/>; <c>false</c> otherwise.
	/// </summary>
	Task<bool> ExistsAsync(Expression<Func<TProjection, bool>> predicate, CancellationToken ct = default);

	/// <summary>
	/// Returns the count of <typeparamref name="TProjection"/> instances matching the given <paramref name="predicate"/>.
	/// When <paramref name="predicate"/> is <c>null</c>, returns the total count.
	/// </summary>
	Task<int> CountAsync(Expression<Func<TProjection, bool>>? predicate = null, CancellationToken ct = default);
}
