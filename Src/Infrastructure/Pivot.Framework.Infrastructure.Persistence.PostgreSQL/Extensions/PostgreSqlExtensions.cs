using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.PostgreSQL.Configuration;

namespace Pivot.Framework.Infrastructure.Persistence.PostgreSQL.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for configuring PostgreSQL-specific persistence features.
///              Registers PostgreSQL JSONB configurations, advisory lock support,
///              and provides a model builder extension for applying PostgreSQL-specific
///              entity configurations.
/// </summary>
public static class PostgreSqlExtensions
{
	/// <summary>
	/// Applies PostgreSQL-specific EF Core configurations for outbox messages and event history.
	/// Call this in your DbContext's <c>OnModelCreating</c> method after applying the base configurations.
	/// </summary>
	/// <param name="modelBuilder">The EF Core model builder.</param>
	/// <returns>The model builder for chaining.</returns>
	public static ModelBuilder ApplyPostgreSqlConfigurations(this ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new PostgreSqlOutboxMessageConfiguration());
		modelBuilder.ApplyConfiguration(new PostgreSqlEventHistoryEntryConfiguration());
		return modelBuilder;
	}

	/// <summary>
	/// Configures a DbContext to use PostgreSQL with Npgsql.
	/// </summary>
	/// <typeparam name="TContext">The DbContext type.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="connectionString">The PostgreSQL connection string.</param>
	/// <param name="configureOptions">Optional Npgsql options configuration.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddPostgreSqlContext<TContext>(
		this IServiceCollection services,
		string connectionString,
		Action<DbContextOptionsBuilder>? configureOptions = null)
		where TContext : DbContext, IPersistenceContext
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

		services.AddDbContext<TContext>(options =>
		{
			options.UseNpgsql(connectionString);
			configureOptions?.Invoke(options);
		});

		return services;
	}
}
