using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pivot.Framework.Containers.API.Health;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering health checks.
///              Provides registration helpers for database, RabbitMQ, and downstream service health.
/// </summary>
public static class HealthCheckExtensions
{
	/// <summary>
	/// Adds a SQL Server database health check.
	/// </summary>
	public static IHealthChecksBuilder AddSqlServerHealthCheck(
		this IHealthChecksBuilder builder,
		string connectionString,
		string name = "sqlserver",
		HealthStatus failureStatus = HealthStatus.Unhealthy)
	{
		return builder.AddCheck(name, new SqlConnectionHealthCheck(connectionString), failureStatus);
	}

	/// <summary>
	/// Adds a PostgreSQL database health check.
	/// </summary>
	public static IHealthChecksBuilder AddPostgreSqlHealthCheck(
		this IHealthChecksBuilder builder,
		string connectionString,
		string name = "postgresql",
		HealthStatus failureStatus = HealthStatus.Unhealthy)
	{
		return builder.AddCheck(name, new PostgreSqlConnectionHealthCheck(connectionString), failureStatus);
	}

	/// <summary>
	/// Adds a RabbitMQ connectivity health check.
	/// </summary>
	public static IHealthChecksBuilder AddRabbitMQHealthCheck(
		this IHealthChecksBuilder builder,
		string connectionString,
		string name = "rabbitmq",
		HealthStatus failureStatus = HealthStatus.Unhealthy)
	{
		return builder.AddCheck(name, new RabbitMQHealthCheck(connectionString), failureStatus);
	}

	/// <summary>
	/// Adds a downstream HTTP service health check.
	/// </summary>
	public static IHealthChecksBuilder AddServiceHealthCheck(
		this IHealthChecksBuilder builder,
		string name,
		string healthEndpointUrl,
		HealthStatus failureStatus = HealthStatus.Degraded)
	{
		return builder.AddCheck(name, new HttpServiceHealthCheck(healthEndpointUrl), failureStatus);
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : SQL Server connectivity health check.
/// </summary>
public sealed class SqlConnectionHealthCheck(string connectionString) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
	{
		try
		{
			await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
			await connection.OpenAsync(ct);
			return HealthCheckResult.Healthy("SQL Server connection successful.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("SQL Server connection failed.", ex);
		}
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : PostgreSQL connectivity health check.
/// </summary>
public sealed class PostgreSqlConnectionHealthCheck(string connectionString) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
	{
		try
		{
			await using var connection = new Npgsql.NpgsqlConnection(connectionString);
			await connection.OpenAsync(ct);
			return HealthCheckResult.Healthy("PostgreSQL connection successful.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
		}
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : RabbitMQ connectivity health check.
/// </summary>
public sealed class RabbitMQHealthCheck(string connectionString) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
	{
		try
		{
			var factory = new RabbitMQ.Client.ConnectionFactory { Uri = new Uri(connectionString) };
			await using var connection = await factory.CreateConnectionAsync(ct);
			return HealthCheckResult.Healthy("RabbitMQ connection successful.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("RabbitMQ connection failed.", ex);
		}
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : HTTP downstream service health check.
/// </summary>
public sealed class HttpServiceHealthCheck(string healthEndpointUrl) : IHealthCheck
{
	private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
	{
		try
		{
			var response = await HttpClient.GetAsync(healthEndpointUrl, ct);
			return response.IsSuccessStatusCode
				? HealthCheckResult.Healthy($"Service at {healthEndpointUrl} is healthy.")
				: HealthCheckResult.Degraded($"Service at {healthEndpointUrl} returned {response.StatusCode}.");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy($"Service at {healthEndpointUrl} is unreachable.", ex);
		}
	}
}
