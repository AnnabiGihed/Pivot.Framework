using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions.ReadModels;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.InProcess;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Projections;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI registration extensions for in-process message publishing and
///              projection support infrastructure.
///
///              Usage:
///              <code>
///              // Transport: no external broker needed
///              services.AddInProcessMessagePublisher();
///
///              // Projection support (safe projection-only dispatch)
///              services.AddProjectionSupport();
///              </code>
/// </summary>
public static class InProcessPublisherExtensions
{
    /// <summary>
    /// Registers <see cref="InProcessMessagePublisher"/> as the <see cref="IMessagePublisher"/>
    /// implementation. Outbox messages are deserialized and dispatched locally via
    /// <c>IDomainEventDispatcher</c> (MediatR) instead of being sent to an external broker.
    ///
    /// This extension selects only the publishing transport.
    /// Outbox draining mode must be configured separately via AddOutboxDraining(...).
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddInProcessMessagePublisher(this IServiceCollection services)
	{
		services.AddSingleton<IMessagePublisher, InProcessMessagePublisher>();
		return services;
	}

	/// <summary>
	/// Registers <see cref="ProjectionDispatcher"/> as the <see cref="IProjectionDispatcher"/>
	/// implementation. This dispatcher resolves and invokes only <see cref="IProjectionHandler{TEvent}"/>
	/// implementations — no side-effect handlers are triggered.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddProjectionSupport(
		this IServiceCollection services)
	{
		services.AddScoped<IProjectionDispatcher, ProjectionDispatcher>();
		return services;
	}
}
