using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Services;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Processor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Modified    : 03-2026 — Added configurable <see cref="OutboxRetryOptions"/> for max retry
///              threshold and dead-letter event emission.
/// Purpose     : Registers the outbox draining mode for the current application.
///              Exactly one mode can be configured:
///              - ImmediateAfterRequest
///              - BackgroundPolling
///              This method is the only supported registration entry point for outbox draining.
/// </summary>
public static class OutboxDrainExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers the outbox draining infrastructure for the given <typeparamref name="TContext"/>.
    /// Configures the selected drain mode (<see cref="OutboxDrainMode.ImmediateAfterRequest"/> or
    /// <see cref="OutboxDrainMode.BackgroundPolling"/>) and throws if called more than once.
    /// </summary>
    /// <typeparam name="TContext">The EF Core DbContext that implements <see cref="IPersistenceContext"/>.</typeparam>
    /// <param name="services">The service collection to register the outbox infrastructure into.</param>
    /// <param name="configure">A delegate to configure <see cref="OutboxDrainOptions"/>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOutboxDraining<TContext>(this IServiceCollection services, Action<OutboxDrainOptions> configure)
        where TContext : DbContext, IPersistenceContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new OutboxDrainOptions();
        configure(options);

        EnforceExclusiveMode(services);

        services.AddSingleton(new OutboxDrainRegistrationMarker(options.Mode));
        services.TryAddScoped<IOutboxProcessor<TContext>, OutboxProcessor<TContext>>();

        // Register default retry options (can be overridden by ConfigureOutboxRetry)
        services.Configure<OutboxRetryOptions>(_ => { });

        if (options.Mode == OutboxDrainMode.BackgroundPolling)
        {
            services.Configure<OutboxPublisherOptions>(o =>
            {
                o.PollingInterval = options.PollingInterval;
            });

            services.AddHostedService<OutboxPublisherService<TContext>>();
        }

        return services;
    }

    /// <summary>
    /// Configures the outbox retry and dead-letter options.
    /// Must be called after <see cref="AddOutboxDraining{TContext}"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">A delegate to configure <see cref="OutboxRetryOptions"/>.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection ConfigureOutboxRetry(this IServiceCollection services, Action<OutboxRetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        return services;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if an outbox drain mode has already been registered,
    /// enforcing that only one drain mode can be active at a time.
    /// </summary>
    /// <param name="services">The service collection to inspect.</param>
    private static void EnforceExclusiveMode(IServiceCollection services)
    {
        var alreadyRegistered = services.Any(x => x.ServiceType == typeof(OutboxDrainRegistrationMarker));

        if (alreadyRegistered)
            throw new InvalidOperationException("An outbox draining mode has already been registered. Only one outbox draining mode can be configured.");
    }

    #endregion
}