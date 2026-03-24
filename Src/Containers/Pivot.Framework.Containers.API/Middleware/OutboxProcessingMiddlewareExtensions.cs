using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Registers middleware-based outbox draining in the ASP.NET Core pipeline.
///              This entry point is valid only when the configured drain mode is
///              ImmediateAfterRequest.
/// </summary>
public static class OutboxProcessingMiddlewareExtensions
{
    public static IApplicationBuilder UseImmediateOutboxDraining<TContext>(this IApplicationBuilder app)
        where TContext : DbContext, IPersistenceContext
    {
        ArgumentNullException.ThrowIfNull(app);

        var marker = app.ApplicationServices.GetService<OutboxDrainRegistrationMarker>();

        if (marker is null)
            throw new InvalidOperationException("Outbox draining has not been configured. Call services.AddOutboxDraining<TContext>(...) first.");

        if (marker.Mode != OutboxDrainMode.ImmediateAfterRequest)
            throw new InvalidOperationException($"The configured outbox draining mode is '{marker.Mode}', but middleware-based draining requires '{OutboxDrainMode.ImmediateAfterRequest}'.");

        return app.UseMiddleware<OutboxProcessingMiddleware<TContext>>();
    }
}