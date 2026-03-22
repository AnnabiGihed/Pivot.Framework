using System.Linq.Expressions;

namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines read-only query operations for read models (projections) in a
///              CQRS architecture. Implementations must execute all queries without
///              change-tracking for optimal read performance.
///
///              This interface is infrastructure-agnostic — it can be implemented by
///              EF Core, MongoDB, Dapper, or any other data-access technology.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IReadModelRepository<TReadModel, in TId>
	where TReadModel : class, IReadModel<TId>
{
	/// <summary>
	/// Returns the <typeparamref name="TReadModel"/> with the given <paramref name="id"/>,
	/// or <c>null</c> when no match is found. The result is read-only (no change-tracking).
	/// </summary>
	Task<TReadModel?> GetByIdAsync(TId id, CancellationToken ct = default);

	/// <summary>
	/// Returns all <typeparamref name="TReadModel"/> instances matching the given <paramref name="predicate"/>.
	/// When <paramref name="predicate"/> is <c>null</c>, returns all read models.
	/// Results are read-only (no change-tracking).
	/// </summary>
	Task<IReadOnlyList<TReadModel>> GetAllAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default);

	/// <summary>
	/// Returns <c>true</c> when at least one <typeparamref name="TReadModel"/> satisfies
	/// <paramref name="predicate"/>; <c>false</c> otherwise.
	/// </summary>
	Task<bool> ExistsAsync(
		Expression<Func<TReadModel, bool>> predicate,
		CancellationToken ct = default);

	/// <summary>
	/// Returns the count of <typeparamref name="TReadModel"/> instances matching the given
	/// <paramref name="predicate"/>. When <paramref name="predicate"/> is <c>null</c>,
	/// returns the total count.
	/// </summary>
	Task<int> CountAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default);

	/// <summary>
	/// Returns read models matching the given <paramref name="specification"/>.
	/// The specification encapsulates criteria, ordering, and paging.
	/// </summary>
	Task<IReadOnlyList<TReadModel>> ListAsync(
		ReadModelSpecification<TReadModel> specification,
		CancellationToken ct = default);
}
