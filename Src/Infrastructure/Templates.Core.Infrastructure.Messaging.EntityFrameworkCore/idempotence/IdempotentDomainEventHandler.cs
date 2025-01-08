using MediatR;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;
using Templates.Core.Application.Abstrations.Messaging.Events;
using Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.idempotence;

public sealed class IdempotentDomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
	private readonly INotificationHandler<TDomainEvent> _decorated;
	private readonly DbContext _dbContext;

	public IdempotentDomainEventHandler(
		INotificationHandler<TDomainEvent> decorated,
		DbContext dbContext)
	{
		_decorated = decorated;
		_dbContext = dbContext;
	}

	public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
	{
		string consumer = _decorated.GetType().Name;

		if (await _dbContext.Set<OutboxMessageConsumer>()
				.AnyAsync(
					outboxMessageConsumer =>
						outboxMessageConsumer.Id == notification.Id &&
						outboxMessageConsumer.Name == consumer,
					cancellationToken))
		{
			return;
		}

		await _decorated.Handle(notification, cancellationToken);

		_dbContext.Set<OutboxMessageConsumer>()
			.Add(new OutboxMessageConsumer
			{
				Id = notification.Id,
				Name = consumer
			});

		await _dbContext.SaveChangesAsync(cancellationToken);
	}
}