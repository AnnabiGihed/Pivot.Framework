using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.IntegrationEvents.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for integration event publishing.
///              Registers <see cref="IIntegrationEventPublisher"/> backed by the outbox,
///              ensuring integration events are persisted atomically with business data
///              and routed to the external message broker.
///
///              Usage:
///              <code>
///              services.AddIntegrationEventPublisher&lt;MyDbContext&gt;();
///              </code>
/// </summary>
public static class IntegrationEventExtensions
{
	/// <summary>
	/// Registers <see cref="IIntegrationEventPublisher"/> → <see cref="IntegrationEventPublisher{TContext}"/>
	/// for the given persistence context.
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext that implements <see cref="IPersistenceContext"/>.</typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddIntegrationEventPublisher<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		services.TryAddScoped<IIntegrationEventPublisher, IntegrationEventPublisher<TContext>>();
		return services;
	}
}
