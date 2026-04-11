using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Versioning;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Projections;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Versioning;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Projections;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Repositories;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering event store infrastructure.
///              Registers the event store repository, projection checkpoint store,
///              event version registry, and projection rebuilder.
/// </summary>
public static class EventStoreExtensions
{
	/// <summary>
	/// Registers the event store infrastructure for the specified DbContext type.
	/// Includes: IEventStoreRepository, IProjectionCheckpointStore, IEventVersionRegistry,
	/// and IProjectionRebuilder.
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext type that owns the event store table.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddEventStore<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddScoped<IEventStoreRepository<TContext>, EventStoreRepository<TContext>>();
		services.TryAddScoped<IProjectionCheckpointStore, ProjectionCheckpointStore>();
		services.TryAddSingleton<IEventVersionRegistry, EventVersionRegistry>();
		services.TryAddScoped<IProjectionRebuilder, ProjectionRebuilder<TContext>>();

		return services;
	}

	/// <summary>
	/// Registers an event upgrader for a specific event type and version transition.
	/// </summary>
	/// <typeparam name="TEvent">The target event type after upgrade.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="upgrader">The event upgrader instance.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddEventUpgrader<TEvent>(this IServiceCollection services, IEventUpgrader<TEvent> upgrader)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(upgrader);

		// Register the upgrader with the EventVersionRegistry at startup
		services.AddSingleton(upgrader);

		return services;
	}
}