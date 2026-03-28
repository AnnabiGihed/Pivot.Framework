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
    #region Public Methods
    /// <summary>
    /// Adds the <see cref="OutboxProcessingMiddleware{TContext}"/> to the ASP.NET Core pipeline.
    /// Only valid when the configured drain mode is <see cref="OutboxDrainMode.ImmediateAfterRequest"/>.
    /// Must be called after <c>services.AddOutboxDraining&lt;TContext&gt;</c> has been registered.
    /// </summary>
    /// <typeparam name="TContext">The EF Core DbContext that implements <see cref="IPersistenceContext"/>.</typeparam>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when outbox draining has not been configured or when the configured mode is not <see cref="OutboxDrainMode.ImmediateAfterRequest"/>.
    /// </exception>
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
    #endregion
}