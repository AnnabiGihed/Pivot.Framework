using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Application.Abstractions.ReadModels;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IReadModelRepository{TReadModel,TId}"/>.
///              All queries run with <c>AsNoTracking</c> for optimal read performance.
///              Designed for CQRS read-side repositories backed by read model / projection tables.
///              Designed for inheritance — concrete repositories may override any virtual method
///              to add custom query behaviour.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public class EfCoreReadModelRepository<TReadModel, TId> : IReadModelRepository<TReadModel, TId>
	where TReadModel : class, IReadModel<TId>
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
	/// Initialises a new <see cref="EfCoreReadModelRepository{TReadModel,TId}"/> with the
	/// provided <see cref="DbContext"/>.
	/// </summary>
	/// <param name="dbContext">The EF Core database context. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext"/> is null.</exception>
	public EfCoreReadModelRepository(DbContext dbContext)
	{
		DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public virtual async Task<TReadModel?> GetByIdAsync(TId id, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(id);

		return await DbContext.Set<TReadModel>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id!.Equals(id), ct);
	}

	/// <inheritdoc />
	public virtual async Task<IReadOnlyList<TReadModel>> GetAllAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		var query = DbContext.Set<TReadModel>().AsNoTracking();

		if (predicate is not null)
			query = query.Where(predicate);

		return await query.ToListAsync(ct);
	}

	/// <inheritdoc />
	public virtual Task<bool> ExistsAsync(
		Expression<Func<TReadModel, bool>> predicate,
		CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(predicate);

		return DbContext.Set<TReadModel>()
			.AsNoTracking()
			.AnyAsync(predicate, ct);
	}

	/// <inheritdoc />
	public virtual async Task<int> CountAsync(
		Expression<Func<TReadModel, bool>>? predicate = null,
		CancellationToken ct = default)
	{
		var query = DbContext.Set<TReadModel>().AsNoTracking();

		if (predicate is not null)
			query = query.Where(predicate);

		return await query.CountAsync(ct);
	}

	/// <inheritdoc />
	public virtual async Task<IReadOnlyList<TReadModel>> ListAsync(
		ReadModelSpecification<TReadModel> specification,
		CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(specification);

		var query = ReadModelSpecificationEvaluator.GetQuery(
			DbContext.Set<TReadModel>(), specification);

		return await query.ToListAsync(ct);
	}
	#endregion
}
