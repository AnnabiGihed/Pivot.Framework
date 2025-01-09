namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Repositories;
public interface IOutboxRepository
{
	Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
	Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default);
}