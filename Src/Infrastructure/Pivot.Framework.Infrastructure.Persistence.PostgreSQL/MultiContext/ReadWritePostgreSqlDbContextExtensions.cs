using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pivot.Framework.Infrastructure.Persistence.PostgreSQL.MultiContext;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : PostgreSQL read/write database separation helper for multi-context deployments.
///              Allows registering a write DbContext (primary) and a read DbContext (replica)
///              with separate PostgreSQL connection strings for CQRS read/write split.
/// </summary>
public static class ReadWritePostgreSqlDbContextExtensions
{
	/// <summary>
	/// Registers separate read and write DbContexts backed by PostgreSQL.
	/// </summary>
	/// <typeparam name="TWriteContext">The DbContext used for write operations (commands).</typeparam>
	/// <typeparam name="TReadContext">The DbContext used for read operations (queries).</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="writeConnectionString">Connection string to the primary (write) database.</param>
	/// <param name="readConnectionString">Connection string to the read replica.</param>
	/// <param name="configureWrite">Optional additional configuration for the write context.</param>
	/// <param name="configureRead">Optional additional configuration for the read context.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddReadWritePostgreSqlDbContexts<TWriteContext, TReadContext>(
		this IServiceCollection services,
		string writeConnectionString,
		string readConnectionString,
		Action<DbContextOptionsBuilder>? configureWrite = null,
		Action<DbContextOptionsBuilder>? configureRead = null)
		where TWriteContext : DbContext
		where TReadContext : DbContext
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentException.ThrowIfNullOrWhiteSpace(writeConnectionString);
		ArgumentException.ThrowIfNullOrWhiteSpace(readConnectionString);

		services.AddDbContext<TWriteContext>(options =>
		{
			options.UseNpgsql(writeConnectionString);
			configureWrite?.Invoke(options);
		});

		services.AddDbContext<TReadContext>(options =>
		{
			options.UseNpgsql(readConnectionString, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			configureRead?.Invoke(options);
		});

		return services;
	}
}
