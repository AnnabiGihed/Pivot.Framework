using Templates.Core.Domain.Primitives;

namespace Templates.Core.Domain.Repositories;

public interface IAsyncQueryRepository<TProjection, TId> where TProjection : ProjectionRoot<TId>
{
	Task<TProjection?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
	Task<TProjection> GetByPredicateAsync(Func<TProjection, bool> predicate, string includeNavigationProperty = default!);
	Task<IReadOnlyList<TProjection>> ListAllAsync(CancellationToken cancellationToken = default);
	Task<TProjection> AddAsync(TProjection entity);
	Task<bool> UpdateAsync(TProjection entity);
	Task DeleteAsync(TProjection entity);
	Task<IReadOnlyList<TProjection>> GetPagedReponseAsync(int page, int size, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
