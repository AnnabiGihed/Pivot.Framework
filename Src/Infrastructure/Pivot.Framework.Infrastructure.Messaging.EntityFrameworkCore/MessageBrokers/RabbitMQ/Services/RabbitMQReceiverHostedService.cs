using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageReceiver;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Services;

public class RabbitMQReceiverHostedService : BackgroundService
{
	#region Properties
	protected volatile bool _isListening;
	protected readonly IMessageReceiver _messageReceiver;
	protected readonly IHostApplicationLifetime _applicationLifetime;
	protected readonly ILogger<RabbitMQReceiverHostedService> _logger;
	#endregion

	#region Constructor
	public RabbitMQReceiverHostedService(IMessageReceiver messageReceiver, ILogger<RabbitMQReceiverHostedService> logger, IHostApplicationLifetime applicationLifetime)
	{
		_messageReceiver = messageReceiver;
		_logger = logger;
		_applicationLifetime = applicationLifetime;
	}
	#endregion

	#region BackgroundService Methods
	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Stopping RabbitMQ receiver...");
		_isListening = false;
		_messageReceiver.Dispose();
		await base.StopAsync(stoppingToken);
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Wait for application startup
		var tcs = new TaskCompletionSource();
		_applicationLifetime.ApplicationStarted.Register(() => tcs.SetResult());
		await tcs.Task;

		if (_isListening)
		{
			_logger.LogInformation("RabbitMQ receiver is already started. Skipping initialization.");
			return;
		}

		try
		{
			_logger.LogInformation("Initializing RabbitMQ receiver...");
			await _messageReceiver.InitializeAsync();
			await _messageReceiver.StartListeningAsync();
			_isListening = true;
			_logger.LogInformation("RabbitMQ receiver started successfully.");

			// Keep running until stopped
			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			// Normal shutdown
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to start RabbitMQ receiver.");
			_isListening = false;
		}
	}
	#endregion
}
