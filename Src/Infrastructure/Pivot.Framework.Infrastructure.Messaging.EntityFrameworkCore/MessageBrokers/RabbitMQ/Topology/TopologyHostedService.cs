using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Topology;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Hosted service that declares the full RabbitMQ topology at application startup.
///              Runs before consumers begin listening to ensure all exchanges, queues, bindings,
///              DLQs, and retry queues exist.
/// </summary>
public sealed class TopologyHostedService : IHostedService
{
    #region Fields
    private readonly ILogger<TopologyHostedService> _logger;
    private readonly IRabbitMQTopologyManager _topologyManager;
    #endregion

    public TopologyHostedService(IRabbitMQTopologyManager topologyManager, ILogger<TopologyHostedService> logger)
	{
		_topologyManager = topologyManager ?? throw new ArgumentNullException(nameof(topologyManager));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Declaring RabbitMQ topology...");
			await _topologyManager.DeclareTopologyAsync(cancellationToken);
			_logger.LogInformation("RabbitMQ topology declared successfully.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to declare RabbitMQ topology. The application may not function correctly.");
			throw;
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}