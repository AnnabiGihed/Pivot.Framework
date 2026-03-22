using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Services;

/// <summary>
/// Configuration options for <see cref="OutboxPublisherService{TContext}"/>.
/// </summary>
public class OutboxPublisherOptions
{
	/// <summary>
	/// The interval between outbox polling cycles. Defaults to 5 seconds.
	/// </summary>
	public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}

public class OutboxPublisherService<TContext>(
	IServiceProvider serviceProvider,
	ILogger<OutboxPublisherService<TContext>> logger,
	IOptions<OutboxPublisherOptions> options) : BackgroundService where TContext : class, IPersistenceContext
{
	#region Properties
	protected readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	protected readonly ILogger<OutboxPublisherService<TContext>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	protected readonly OutboxPublisherOptions _options = options?.Value ?? new OutboxPublisherOptions();
	#endregion

	#region BackgroundService Overrides
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
							_logger.LogWarning("Failed to publish message {MessageId}: {Error}. Will retry next cycle.",
								message.Id, result.Error);
							continue; // skip marking as processed, will be retried
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
}
