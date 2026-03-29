using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;
using Pivot.Framework.Infrastructure.Abstraction.Inbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox.Behaviors;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox.Repositories;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Inbox.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for the inbox pattern (consumer-side idempotency).
///              Registers the inbox repository, inbox service, and optionally the
///              <see cref="IdempotentCommandBehavior{TRequest,TResponse}"/> MediatR pipeline.
///
///              Usage:
///              <code>
///              // Register inbox support for consumer-side deduplication
///              services.AddInboxSupport&lt;MyDbContext&gt;();
///
///              // Optionally add idempotent command pipeline behavior
///              services.AddIdempotentCommands();
///              </code>
/// </summary>
public static class InboxExtensions
{
	/// <summary>
	/// Registers the inbox pattern infrastructure for the given <typeparamref name="TContext"/>.
	/// This includes:
	/// <list type="bullet">
	///   <item><see cref="IInboxRepository{TContext}"/> → <see cref="InboxRepository{TContext}"/></item>
	///   <item><see cref="IInboxService"/> → <see cref="InboxService{TContext}"/></item>
	/// </list>
	/// The inbox enables consumer-side message deduplication in both
	/// <c>RabbitMQReceiver</c> and <c>InProcessMessagePublisher</c>.
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext that implements <see cref="IPersistenceContext"/>.</typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddInboxSupport<TContext>(this IServiceCollection services)
		where TContext : DbContext, IPersistenceContext
	{
		services.TryAddScoped(typeof(IInboxRepository<>), typeof(InboxRepository<>));
		services.TryAddScoped<IInboxService, InboxService<TContext>>();

		return services;
	}

	/// <summary>
	/// Registers the <see cref="IdempotentCommandBehavior{TRequest,TResponse}"/> MediatR
	/// pipeline behavior. Commands implementing <c>IIdempotentCommand</c> will be
	/// automatically deduplicated using the inbox pattern.
	///
	/// Requires <see cref="AddInboxSupport{TContext}"/> to have been called first.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddIdempotentCommands(this IServiceCollection services)
	{
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotentCommandBehavior<,>));
		return services;
	}
}
