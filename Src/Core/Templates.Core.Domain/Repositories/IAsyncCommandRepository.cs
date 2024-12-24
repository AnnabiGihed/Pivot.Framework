using CSharpFunctionalExtensions;

namespace Templates.Core.Domain.Repositories;

public interface IAsyncCommandRepository<TEntity, TId> where TEntity : Entity<TId> where TId : IComparable<TId>
{
	Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
	Task<TEntity> GetByPredicateAsync(Func<TEntity, bool> predicate, string includeNavigationProperty = default!);
	Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default);
	Task<TEntity> AddAsync(TEntity entity);
	Task UpdateAsync(TEntity entity);
	Task DeleteAsync(TEntity entity);
	Task<IReadOnlyList<TEntity>> GetPagedReponseAsync(int page, int size, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
