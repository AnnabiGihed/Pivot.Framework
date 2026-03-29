using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.Audit;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Audit.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering the audit log infrastructure.
///              Registers the EF Core-backed AuditLogService for persisting administrative
///              action audit trails.
/// </summary>
public static class AuditLogExtensions
{
	/// <summary>
	/// Registers the audit log service using the specified DbContext.
	/// The DbContext must have <see cref="AuditEntry"/> configured via
	/// <see cref="Configuration.AuditEntryConfiguration"/>.
	/// </summary>
	/// <typeparam name="TContext">The EF Core DbContext that owns the AuditLog table.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddAuditLog<TContext>(this IServiceCollection services)
		where TContext : DbContext
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddScoped<IAuditLogService>(sp =>
			new AuditLogService(sp.GetRequiredService<TContext>()));

		return services;
	}
}
