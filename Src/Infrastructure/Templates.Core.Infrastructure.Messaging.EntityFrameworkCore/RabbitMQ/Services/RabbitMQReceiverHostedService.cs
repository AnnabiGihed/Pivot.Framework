using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageReceiver;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.Services;

public class RabbitMQReceiverHostedService : BackgroundService
{
	private readonly IMessageReceiver _messageReceiver;
	private readonly ILogger<RabbitMQReceiverHostedService> _logger;
	private readonly IHostApplicationLifetime _applicationLifetime;
	private readonly object _lock = new();
	private bool _isListening = false;

	public RabbitMQReceiverHostedService(
		IMessageReceiver messageReceiver,
		ILogger<RabbitMQReceiverHostedService> logger,
		IHostApplicationLifetime applicationLifetime)
	{
		_messageReceiver = messageReceiver;
		_logger = logger;
		_applicationLifetime = applicationLifetime;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_applicationLifetime.ApplicationStarted.Register(() =>
		{
			_logger.LogInformation("Application started. Preparing to initialize RabbitMQ receiver...");

			lock (_lock)
			{
				if (_isListening)
				{
					_logger.LogInformation("RabbitMQ receiver is already started. Skipping initialization.");
					return;
				}

				_isListening = true; // Mark as started
			}

			Task.Run(async () =>
			{
				try
				{
					_logger.LogInformation("Initializing RabbitMQ receiver...");
					await _messageReceiver.InitializeAsync();
					await _messageReceiver.StartListeningAsync();
					_logger.LogInformation("RabbitMQ receiver is now listening.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to start RabbitMQ receiver.");
					lock (_lock)
					{
						_isListening = false; // Reset the flag if initialization fails
					}
				}
			});
		});
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Stopping RabbitMQ receiver...");
		lock (_lock)
		{
			_isListening = false;
		}
		_messageReceiver.Dispose();
		await base.StopAsync(stoppingToken);
	}
}