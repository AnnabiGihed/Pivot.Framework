using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Processor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Processes unprocessed outbox messages by publishing them to the message broker.
///              Iterates over pending messages, publishes each one, and marks it as processed.
///              If publishing fails, the message's retry count is incremented and the failure
///              is returned immediately so that callers can decide on retry strategy.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that stores the outbox table.</typeparam>
public sealed class OutboxProcessor<TContext>(
	IOutboxRepository<TContext> outboxRepository,
	IMessagePublisher messagePublisher,
	TContext dbContext)
	: IOutboxProcessor<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Public Methods
	/// <summary>
	/// Retrieves all unprocessed outbox messages and publishes them to the message broker.
	/// Each successfully published message is marked as processed. On failure, the message's
	/// retry count is incremented and a failure result is returned.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or the first encountered failure.</returns>
	public async Task<Result> ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
	{
		try
		{
			var messages = await outboxRepository.GetUnprocessedMessagesAsync(cancellationToken);

			if (messages.Count == 0)
				return Result.Success();

			foreach (var message in messages)
			{
				var publishResult = await messagePublisher.PublishAsync(message);

				if (publishResult.IsFailure)
				{
					message.RetryCount++;
					await dbContext.SaveChangesAsync(cancellationToken);
					return publishResult;
				}

				var markResult = await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
				if (markResult.IsFailure)
					return markResult;

				await dbContext.SaveChangesAsync(cancellationToken);
			}

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("OutboxProcessingError", ex.Message));
		}
	}
	#endregion
}
