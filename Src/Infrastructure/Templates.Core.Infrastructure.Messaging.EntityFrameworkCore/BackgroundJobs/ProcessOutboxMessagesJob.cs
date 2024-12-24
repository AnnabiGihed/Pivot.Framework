using Quartz;
using MediatR;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;
using Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
	private readonly DbContext _dbContext;
	private readonly IPublisher _publisher;

	public ProcessOutboxMessagesJob(DbContext dbContext, IPublisher publisher)
	{
		_dbContext = dbContext;
		_publisher = publisher;
	}

	/// <summary>
	/// TODO: Add exception handling and logging.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public virtual async Task Execute(IJobExecutionContext context)
	{
		List<OutboxMessage> messages = await _dbContext
			.Set<OutboxMessage>()
			.Where(m => m.ProcessedOnUtc == null)
			.Take(20)
			.ToListAsync(context.CancellationToken);

		foreach (OutboxMessage outboxMessage in messages)
		{
			IDomainEvent? domainEvent = JsonConvert
				.DeserializeObject<IDomainEvent>(
					outboxMessage.Content,
					new JsonSerializerSettings
					{
						TypeNameHandling = TypeNameHandling.All
					});

			if (domainEvent is null)
			{
				continue;
			}

			await _publisher.Publish(domainEvent, context.CancellationToken);

			outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
		}

		await _dbContext.SaveChangesAsync();
	}
}
