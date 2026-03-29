using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Processor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Added configurable max retry threshold with dead-letter support.
///              When a message exceeds <see cref="OutboxRetryOptions.MaxRetryCount"/>, it is
///              permanently marked as failed and an <see cref="OutboxMessageFailedEvent"/> is
///              optionally emitted to the outbox for downstream compensation.
/// Purpose     : Processes unprocessed outbox messages by publishing them to the message broker.
///              Iterates over pending messages, publishes each one, and marks it as processed.
///              If publishing fails, the message's retry count is incremented. After exceeding
///              the configured max retries, the message is dead-lettered.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that stores the outbox table.</typeparam>
public sealed class OutboxProcessor<TContext>(
	IOutboxRepository<TContext> outboxRepository,
	IMessagePublisher messagePublisher,
	TContext dbContext,
	ILogger<OutboxProcessor<TContext>> logger,
	IOptions<OutboxRetryOptions> retryOptions)
	: IOutboxProcessor<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Fields

	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		Formatting = Formatting.None
	};

	private readonly OutboxRetryOptions _retryOptions = retryOptions?.Value ?? new OutboxRetryOptions();

	#endregion

	#region Public Methods
	/// <summary>
	/// Retrieves all unprocessed outbox messages and publishes them to the message broker.
	/// Each successfully published message is marked as processed. On failure, the message's
	/// retry count is incremented. Messages that exceed <see cref="OutboxRetryOptions.MaxRetryCount"/>
	/// are permanently dead-lettered and optionally trigger an <see cref="OutboxMessageFailedEvent"/>.
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
					message.LastError = publishResult.Error?.Message;

					// ── Dead-letter threshold check ─────────────────────────────
					if (message.RetryCount >= _retryOptions.MaxRetryCount)
					{
						await DeadLetterMessageAsync(message, cancellationToken);
					}

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

	#region Private Methods

	/// <summary>
	/// Permanently marks a message as failed (dead-lettered) and optionally emits an
	/// <see cref="OutboxMessageFailedEvent"/> to the outbox for downstream compensation.
	/// </summary>
	private async Task DeadLetterMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
	{
		message.Processed = true;
		message.FailedAtUtc = DateTime.UtcNow;

		logger.LogError(
			"Outbox message {MessageId} (type: {EventType}) permanently failed after {RetryCount} retries. Last error: {LastError}",
			message.Id, message.EventType, message.RetryCount, message.LastError);

		if (_retryOptions.EmitFailureEvent)
		{
			var failureEvent = new OutboxMessageFailedEvent(
				failedMessageId: message.Id,
				eventType: message.EventType ?? "Unknown",
				retryCount: message.RetryCount,
				lastError: message.LastError,
				correlationId: message.CorrelationId);

			var failureMessage = new OutboxMessage
			{
				Id = failureEvent.Id,
				Payload = JsonConvert.SerializeObject(failureEvent, SerializerSettings),
				EventType = failureEvent.GetType().AssemblyQualifiedName,
				CreatedAtUtc = failureEvent.OccurredOnUtc,
				CorrelationId = message.CorrelationId,
				Kind = MessageKind.IntegrationEvent
			};

			await outboxRepository.AddAsync(failureMessage, cancellationToken);

			logger.LogInformation(
				"Emitted OutboxMessageFailedEvent {FailureEventId} for dead-lettered message {OriginalMessageId}.",
				failureEvent.Id, message.Id);
		}
	}

	#endregion
}
