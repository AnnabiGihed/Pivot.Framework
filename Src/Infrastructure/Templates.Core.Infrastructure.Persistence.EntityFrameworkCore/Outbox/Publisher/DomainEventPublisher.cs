using Newtonsoft.Json;
using Templates.Core.Domain.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Templates.Core.Domain.Primitives;
using Templates.Core.Infrastructure.Abstraction.Outbox.Models;
using Templates.Core.Infrastructure.Abstraction.Outbox.Repositories;
using Templates.Core.Infrastructure.Abstraction.Outbox.DomainEventPublisher;

namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

public class DomainEventPublisher<TContext>(IOutboxRepository<TContext> outboxRepository, ILogger<DomainEventPublisher<TContext>> logger) : IDomainEventPublisher where TContext : DbContext
{
	#region Properties
	protected readonly IOutboxRepository<TContext> _outboxRepository = outboxRepository;
	protected readonly ILogger<DomainEventPublisher<TContext>> _logger = logger;
	#endregion

	#region IDomainEventPublisher Implementation
	public async Task<Result> PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(domainEvent);

			var serializedObject = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Prevents circular references
				Formatting = Formatting.None, // Compact JSON
				StringEscapeHandling = StringEscapeHandling.Default // Avoids unnecessary escaping
			});

			var outboxMessage = new OutboxMessage
			{
				Id = domainEvent.Id,
				Payload = serializedObject,
				CreatedAtUtc = domainEvent.OccurredOnUtc,
				EventType = domainEvent?.GetType()?.AssemblyQualifiedName,
			};

			_logger.LogWarning($"Publishing domain event Type: {domainEvent?.GetType()?.AssemblyQualifiedName}");


			return await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("DomainEventPublishError", $"Error while publishing domain event: {ex.Message}"));
		}
	}
	#endregion
}