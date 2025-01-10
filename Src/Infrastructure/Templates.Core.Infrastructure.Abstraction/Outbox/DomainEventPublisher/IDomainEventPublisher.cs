using Templates.Core.Domain.Shared;
using Templates.Core.Domain.Primitives;

namespace Templates.Core.Infrastructure.Abstraction.Outbox.DomainEventPublisher;

public interface IDomainEventPublisher
{
	Task<Result> PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
