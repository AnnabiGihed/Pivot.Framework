using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.DomainEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extension for the domain event dispatcher.
///              Consumers call <see cref="AddDomainEventDispatcher"/> during startup
///              to register the MediatR-based implementation of <see cref="IDomainEventDispatcher"/>.
/// </summary>
public static class DomainEventDispatcherExtensions
{
	/// <summary>
	/// Registers <see cref="MediatRDomainEventDispatcher"/> as the <see cref="IDomainEventDispatcher"/>
	/// implementation. Registered as scoped to align with the MediatR and EF Core lifetimes.
	/// </summary>
	public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
	{
		services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
		return services;
	}
}
