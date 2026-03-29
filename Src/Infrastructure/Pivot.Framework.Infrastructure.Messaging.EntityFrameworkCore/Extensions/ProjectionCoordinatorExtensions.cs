using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Coordinator;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering the Projection Coordinator.
///              The coordinator manages the full projection lifecycle: registration,
///              rebuild planning, parity checking, and version promotion.
/// </summary>
public static class ProjectionCoordinatorExtensions
{
	/// <summary>
	/// Registers the Projection Coordinator for the specified DbContext type.
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext that stores projection registrations.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddProjectionCoordinator<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddScoped<IProjectionCoordinator, ProjectionCoordinator<TContext>>();

		return services;
	}
}
