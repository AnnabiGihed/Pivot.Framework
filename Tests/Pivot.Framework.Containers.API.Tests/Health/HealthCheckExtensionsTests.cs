using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pivot.Framework.Containers.API.Health;

namespace Pivot.Framework.Containers.API.Tests.Health;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="HealthCheckExtensions"/>.
///              Verifies that health checks are registered with correct names and failure statuses.
/// </summary>
public class HealthCheckExtensionsTests
{
	#region AddSqlServerHealthCheck Tests

	[Fact]
	public void AddSqlServerHealthCheck_ShouldRegisterHealthCheck()
	{
		var services = new ServiceCollection();
		var builder = services.AddHealthChecks();

		builder.AddSqlServerHealthCheck("Server=localhost;Database=test;");

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
		options.Value.Registrations.Should().Contain(r => r.Name == "sqlserver");
	}

	[Fact]
	public void AddSqlServerHealthCheck_WithCustomName_ShouldUseIt()
	{
		var services = new ServiceCollection();
		var builder = services.AddHealthChecks();

		builder.AddSqlServerHealthCheck("Server=localhost;", name: "custom-sql");

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
		options.Value.Registrations.Should().Contain(r => r.Name == "custom-sql");
	}

	#endregion

	#region AddPostgreSqlHealthCheck Tests

	[Fact]
	public void AddPostgreSqlHealthCheck_ShouldRegisterHealthCheck()
	{
		var services = new ServiceCollection();
		var builder = services.AddHealthChecks();

		builder.AddPostgreSqlHealthCheck("Host=localhost;Database=test;");

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
		options.Value.Registrations.Should().Contain(r => r.Name == "postgresql");
	}

	#endregion

	#region AddRabbitMQHealthCheck Tests

	[Fact]
	public void AddRabbitMQHealthCheck_ShouldRegisterHealthCheck()
	{
		var services = new ServiceCollection();
		var builder = services.AddHealthChecks();

		builder.AddRabbitMQHealthCheck("amqp://guest:guest@localhost:5672/");

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
		options.Value.Registrations.Should().Contain(r => r.Name == "rabbitmq");
	}

	#endregion

	#region AddServiceHealthCheck Tests

	[Fact]
	public void AddServiceHealthCheck_ShouldRegisterWithDegradedDefault()
	{
		var services = new ServiceCollection();
		var builder = services.AddHealthChecks();

		builder.AddServiceHealthCheck("downstream-api", "http://localhost:5001/health");

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();
		var registration = options.Value.Registrations.FirstOrDefault(r => r.Name == "downstream-api");
		registration.Should().NotBeNull();
		registration!.FailureStatus.Should().Be(HealthStatus.Degraded);
	}

	#endregion
}
