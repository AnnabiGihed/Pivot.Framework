using Templates.Core.Domain.Shared;

namespace Templates.Core.Domain.Repositories;

public interface IUnitOfWork<TId>
{
	Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}

