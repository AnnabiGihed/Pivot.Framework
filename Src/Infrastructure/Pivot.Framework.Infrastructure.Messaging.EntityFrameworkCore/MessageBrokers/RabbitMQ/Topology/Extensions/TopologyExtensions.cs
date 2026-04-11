using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Topology.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering RabbitMQ topology management.
///              Declares all exchanges, queues, bindings, DLQs, and retry delay queues at startup.
/// </summary>
public static class TopologyExtensions
{
	/// <summary>
	/// Registers RabbitMQ topology management with the specified exchange/queue bindings.
	/// A hosted service will declare the full topology at application startup.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">A delegate to configure the topology bindings.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddRabbitMQTopology(this IServiceCollection services, Action<TopologyOptions> configure)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configure);

		services.Configure(configure);
		services.TryAddSingleton<IRabbitMQTopologyManager, RabbitMQTopologyManager>();
		services.AddHostedService<TopologyHostedService>();

		return services;
	}
}