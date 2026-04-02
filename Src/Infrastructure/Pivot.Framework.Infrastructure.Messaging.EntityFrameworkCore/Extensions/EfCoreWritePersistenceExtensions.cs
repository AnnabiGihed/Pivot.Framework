using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Application.Abstractions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;
using Pivot.Framework.Infrastructure.Abstraction.UnitOfWork;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Services;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Transaction;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Registers the transport-agnostic EF Core write-side persistence stack
///              required by command handling and transactional outbox workflows.
///              This includes transaction management, outbox persistence, domain event
///              publishing, current-user resolution for audit stamping, and the
///              application-specific unit of work.
/// </summary>
public static class EfCoreWritePersistenceExtensions
{
	/// <summary>
	/// Registers the transport-agnostic EF Core write-side persistence services for the
	/// specified persistence context and concrete unit of work implementation.
	/// </summary>
	/// <typeparam name="TContext">The EF Core persistence context.</typeparam>
	/// <typeparam name="TUnitOfWork">
	/// The concrete unit of work implementation to expose as <see cref="IUnitOfWork{TContext}"/>.
	/// </typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="includeEventStore">
	/// When <c>true</c>, also registers the event store infrastructure by composing
	/// <see cref="EventStoreExtensions.AddEventStore{TContext}(IServiceCollection)"/>.
	/// </param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddEfCoreWritePersistence<TContext, TUnitOfWork>(
		this IServiceCollection services,
		bool includeEventStore = false)
		where TContext : DbContext, IPersistenceContext
		where TUnitOfWork : class, IUnitOfWork<TContext>
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddHttpContextAccessor();

		services.TryAddScoped<ITransactionManager<TContext>, TransactionManager<TContext>>();
		services.TryAddScoped<IOutboxRepository<TContext>, OutboxRepository<TContext>>();
		services.TryAddScoped<IDomainEventPublisher<TContext>, DomainEventPublisher<TContext>>();
		services.TryAddScoped<IDomainEventPublisher>(sp => sp.GetRequiredService<IDomainEventPublisher<TContext>>());
		services.TryAddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();
		services.TryAddScoped<IUnitOfWork<TContext>, TUnitOfWork>();

		if (includeEventStore)
		{
			services.AddEventStore<TContext>();
		}

		return services;
	}
}
