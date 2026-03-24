using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Services;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Processor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Registers the outbox draining mode for the current application.
///              Exactly one mode can be configured:
///              - ImmediateAfterRequest
///              - BackgroundPolling
///              This method is the only supported registration entry point for outbox draining.
/// </summary>
public static class OutboxDrainExtensions
{
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

    private static void EnforceExclusiveMode(IServiceCollection services)
    {
        var alreadyRegistered = services.Any(x => x.ServiceType == typeof(OutboxDrainRegistrationMarker));

        if (alreadyRegistered)        
            throw new InvalidOperationException("An outbox draining mode has already been registered. Only one outbox draining mode can be configured.");
    }
}