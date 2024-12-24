namespace Templates.Core.Domain.Repositories;

public interface IUnitOfWork<TId>
{
	Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
