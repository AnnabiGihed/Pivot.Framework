using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Routing;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Registers service-specific outbox routing resolution for RabbitMQ publishing.
///              The resolver is singleton because the RabbitMQ publisher is registered as a
///              singleton and must not capture scoped dependencies.
/// </summary>
public static class RabbitMqRoutingExtensions
{
	/// <summary>
	/// Registers the <see cref="IOutboxRoutingResolver"/> implementation used to resolve
	/// per-message RabbitMQ routes.
	/// </summary>
	/// <typeparam name="TResolver">The resolver implementation type.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddOutboxRoutingResolver<TResolver>(this IServiceCollection services)
		where TResolver : class, IOutboxRoutingResolver
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddSingleton<IOutboxRoutingResolver, TResolver>();
		return services;
	}
}
