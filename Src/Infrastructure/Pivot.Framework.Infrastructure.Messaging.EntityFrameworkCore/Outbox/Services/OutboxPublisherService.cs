using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration options for <see cref="OutboxPublisherService{TContext}"/>.
/// </summary>
public class OutboxPublisherOptions
{
	/// <summary>
	/// The interval between outbox polling cycles. Defaults to 5 seconds.
	/// </summary>
	public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Added configurable max retry threshold with dead-letter support.
///              When a message exceeds <see cref="OutboxRetryOptions.MaxRetryCount"/>, it is
///              permanently marked as failed and an <see cref="OutboxMessageFailedEvent"/> is
///              optionally emitted for downstream compensation.
/// Purpose     : Background service that polls the outbox for unprocessed messages and publishes
///              them via <see cref="IMessagePublisher"/>. Successfully published messages are
///              marked as processed. Failed messages have their retry count incremented and are
///              dead-lettered after exceeding the configured threshold.
///              Polling interval is configurable via <see cref="OutboxPublisherOptions"/>.
/// </summary>
/// <typeparam name="TContext">The persistence context type for outbox repository resolution.</typeparam>
internal sealed class OutboxPublisherService<TContext>(
	IServiceProvider serviceProvider,
	ILogger<OutboxPublisherService<TContext>> logger,
	IOptions<OutboxPublisherOptions> options,
	IOptions<OutboxRetryOptions> retryOptions)
	: BackgroundService where TContext : class, IPersistenceContext
{
	#region Fields

	/// <summary>Service provider for creating scoped DI containers per processing cycle.</summary>
	protected readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <summary>Logger for diagnostic tracing of outbox publishing operations.</summary>
	protected readonly ILogger<OutboxPublisherService<TContext>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>Configuration options controlling polling interval.</summary>
	protected readonly OutboxPublisherOptions _options = options?.Value ?? new OutboxPublisherOptions();

	/// <summary>Configuration options controlling retry thresholds and failure event emission.</summary>
	protected readonly OutboxRetryOptions _retryOptions = retryOptions?.Value ?? new OutboxRetryOptions();

	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		Formatting = Formatting.None
	};

	#endregion

	#region BackgroundService Overrides

	/// <summary>
	/// Continuously polls the outbox for unprocessed messages and publishes them.
	/// Each cycle creates a new DI scope for proper scoped service resolution.
	/// Messages that exceed the configured max retry count are dead-lettered.
	/// </summary>
	/// <param name="stoppingToken">Token that signals when shutdown is requested.</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();
				var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository<TContext>>();
				var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

				var messages = await outboxRepository.GetUnprocessedMessagesAsync(stoppingToken);

				foreach (var message in messages)
				{
					try
					{
						var result = await messagePublisher.PublishAsync(message);
						if (result.IsFailure)
						{
							message.RetryCount++;
							message.LastError = result.Error?.Message;

							// ── Dead-letter threshold check ─────────────────────
							if (message.RetryCount >= _retryOptions.MaxRetryCount)
							{
								await DeadLetterMessageAsync(message, outboxRepository, stoppingToken);
							}

							_logger.LogWarning(
								"Failed to publish message {MessageId} (retry {RetryCount}/{MaxRetry}): {Error}.",
								message.Id, message.RetryCount, _retryOptions.MaxRetryCount, result.Error);
							continue;
						}

						await outboxRepository.MarkAsProcessedAsync(message.Id, stoppingToken);
						_logger.LogInformation("Message {MessageId} published successfully.", message.Id);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error publishing message {MessageId}.", message.Id);
					}
				}
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				_logger.LogError(ex, "Error during outbox processing cycle.");
			}

			await Task.Delay(_options.PollingInterval, stoppingToken);
		}
	}
	#endregion

	#region Private Methods

	/// <summary>
	/// Permanently marks a message as failed (dead-lettered) and optionally emits an
	/// <see cref="OutboxMessageFailedEvent"/> to the outbox for downstream compensation.
	/// </summary>
	private async Task DeadLetterMessageAsync(
		OutboxMessage message,
		IOutboxRepository<TContext> outboxRepository,
		CancellationToken cancellationToken)
	{
		message.Processed = true;
		message.FailedAtUtc = DateTime.UtcNow;

		_logger.LogError(
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

			_logger.LogInformation(
				"Emitted OutboxMessageFailedEvent {FailureEventId} for dead-lettered message {OriginalMessageId}.",
				failureEvent.Id, message.Id);
		}
	}

	#endregion
}
