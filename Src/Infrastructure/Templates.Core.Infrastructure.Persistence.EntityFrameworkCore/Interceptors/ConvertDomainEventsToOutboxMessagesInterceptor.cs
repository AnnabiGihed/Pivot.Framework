using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor<TId>
	 : SaveChangesInterceptor where TId : IComparable<TId>
{
	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		DbContext? dbContext = eventData.Context;

		if (dbContext is null)
		{
			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}

		var outboxMessages = dbContext.ChangeTracker
			.Entries<AggregateRoot<TId>>()
			.Select(x => x.Entity)
			.SelectMany(aggregateRoot =>
			{
				var domainEvents = aggregateRoot.GetDomainEvents();

				aggregateRoot.ClearDomainEvents();

				return domainEvents;
			})
			.Select(domainEvent => new OutboxMessage
			{
				Id = Guid.NewGuid(),
				OccurredOnUtc = DateTime.UtcNow,
				Type = domainEvent.GetType().Name,
				Content = JsonConvert.SerializeObject(
					domainEvent,
					new JsonSerializerSettings
					{
						TypeNameHandling = TypeNameHandling.All
					})
			})
			.ToList();

		dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
