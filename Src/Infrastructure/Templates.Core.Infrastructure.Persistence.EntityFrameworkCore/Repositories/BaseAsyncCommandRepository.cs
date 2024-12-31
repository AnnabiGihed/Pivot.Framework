using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Repositories;
using Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Specifications;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
public class BaseAsyncCommandRepository<TEntity, TId> : IAsyncCommandRepository<TEntity, TId> where TEntity : Entity<TId> where TId : IComparable<TId>
{
	protected readonly DbContext _dbContext;

	public BaseAsyncCommandRepository(DbContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public async Task<TEntity> GetByPredicateAsync(Func<TEntity, bool> predicate, string includeNavigationProperty = default!)
	{
		TEntity? t;

		t = (includeNavigationProperty != default) ?
			_dbContext.Set<TEntity>().AsNoTracking().Include(includeNavigationProperty).Where(predicate).FirstOrDefault()
			:
			_dbContext.Set<TEntity>().AsNoTracking().Where(predicate).FirstOrDefault();

		return t == null ? throw new ArgumentNullException(nameof(t)) :
			await Task.FromResult(t);
	}

	public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
	{
		TEntity? t = await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
		return t;
	}

	public async Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
	}

	public virtual async Task<IReadOnlyList<TEntity>> GetPagedReponseAsync(int page, int size, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<TEntity>().AsNoTracking().Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync(cancellationToken);
	}

	public async Task<TEntity> AddAsync(TEntity entity)
	{
		await _dbContext.Set<TEntity>().AddAsync(entity);
		return entity;
	}

	public async Task<bool> UpdateAsync(TEntity entity)
	{
		await Task.Run(() =>
		{
			_dbContext.Entry(entity).State = EntityState.Modified;
		});

		return true;
	}

	public async Task DeleteAsync(TEntity entity)
	{
		await Task.Run(() =>
		{
			_dbContext.Set<TEntity>().Remove(entity);
		});
	}

	public async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
	{
		TEntity? t = await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
		return t != null;
	}

	protected IQueryable<TEntity> ApplySpecification(
	EntitySpecification<TEntity, TId> specification)
	{
		return EntitySpecificationEvaluator<TId>.GetQuery(
			_dbContext.Set<TEntity>(),
			specification);
	}
}