using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.IntegrationEventMapping;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.IntegrationEvents.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Registers the integration-event mapping pipeline that translates domain
///              events into integration events and enqueues them in the same transaction.
///              This feature is opt-in so existing write-side behaviour remains unchanged.
/// </summary>
public static class IntegrationEventMappingExtensions
{
	/// <summary>
	/// Registers the integration-event mapping coordinator and ensures the integration event
	/// publisher is available for the specified persistence context.
	/// </summary>
	/// <typeparam name="TContext">The EF Core persistence context.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddIntegrationEventMapping<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddIntegrationEventPublisher<TContext>();
		services.TryAddScoped<IIntegrationEventMappingCoordinator<TContext>, IntegrationEventMappingCoordinator<TContext>>();

		return services;
	}
}