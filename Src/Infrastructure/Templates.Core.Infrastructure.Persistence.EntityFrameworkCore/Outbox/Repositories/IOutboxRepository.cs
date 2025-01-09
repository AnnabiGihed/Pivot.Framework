using Microsoft.EntityFrameworkCore;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Repositories;
public interface IOutboxRepository<TContext> where TContext : DbContext
{
	Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
	Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default);
}