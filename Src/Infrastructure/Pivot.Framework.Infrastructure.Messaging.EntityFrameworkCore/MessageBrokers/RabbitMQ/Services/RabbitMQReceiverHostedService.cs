using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageReceiver;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : ASP.NET Core hosted service that initialises and manages the RabbitMQ message receiver.
///              Waits for application startup, then initialises the receiver and begins listening.
///              Gracefully stops and disposes the receiver on application shutdown.
/// </summary>
public class RabbitMQReceiverHostedService : BackgroundService
{
	#region Fields
	/// <summary>
	/// Indicates whether the receiver is currently listening for messages.
	/// </summary>
	protected volatile bool _isListening;

	/// <summary>
	/// The message receiver that handles RabbitMQ message consumption.
	/// </summary>
	protected readonly IMessageReceiver _messageReceiver;

	/// <summary>
	/// Provides notifications about application lifetime events.
	/// </summary>
	protected readonly IHostApplicationLifetime _applicationLifetime;

	/// <summary>
	/// Logger for diagnostic tracing of receiver lifecycle events.
	/// </summary>
	protected readonly ILogger<RabbitMQReceiverHostedService> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="RabbitMQReceiverHostedService"/> with the provided dependencies.
	/// </summary>
	/// <param name="messageReceiver">The message receiver. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	/// <param name="applicationLifetime">The application lifetime notifier. Must not be null.</param>
	public RabbitMQReceiverHostedService(IMessageReceiver messageReceiver, ILogger<RabbitMQReceiverHostedService> logger, IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
		_applicationLifetime = applicationLifetime;
	}
	#endregion

	#region BackgroundService Methods
	/// <summary>
	/// Stops the RabbitMQ receiver and disposes the underlying connection.
	/// </summary>
	/// <param name="stoppingToken">Token that signals when shutdown is requested.</param>
	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Stopping RabbitMQ receiver...");
		_isListening = false;
		_messageReceiver.Dispose();
		await base.StopAsync(stoppingToken);
	}

	/// <summary>
	/// Waits for application startup, then initialises the RabbitMQ receiver
	/// and starts listening for messages. Runs until cancellation is requested.
	/// </summary>
	/// <param name="stoppingToken">Token that signals when shutdown is requested.</param>
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