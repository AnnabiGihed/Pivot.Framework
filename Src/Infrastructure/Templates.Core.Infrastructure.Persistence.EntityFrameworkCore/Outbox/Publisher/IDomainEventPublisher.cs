using Templates.Core.Domain.Primitives;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

public interface IDomainEventPublisher
{
	Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
