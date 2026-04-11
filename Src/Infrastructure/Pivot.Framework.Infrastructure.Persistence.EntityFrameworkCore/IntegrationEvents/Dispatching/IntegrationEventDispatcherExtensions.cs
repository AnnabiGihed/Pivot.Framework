using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.IntegrationEvents.Dispatching;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : DI registration extension for the integration event dispatcher.
///              Consumers call <see cref="AddIntegrationEventDispatcher"/> during startup
///              to register the MediatR-based implementation of <see cref="IIntegrationEventDispatcher"/>.
/// </summary>
public static class IntegrationEventDispatcherExtensions
{
	#region Public Methods
	/// <summary>
	/// Registers <see cref="MediatRIntegrationEventDispatcher"/> as the <see cref="IIntegrationEventDispatcher"/>
	/// implementation. Registered as scoped to align with the MediatR and EF Core lifetimes.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The same service collection for chaining.</returns>
	public static IServiceCollection AddIntegrationEventDispatcher(this IServiceCollection services)
	{
		services.AddScoped<IIntegrationEventDispatcher, MediatRIntegrationEventDispatcher>();
		return services;
	}
	#endregion
}
