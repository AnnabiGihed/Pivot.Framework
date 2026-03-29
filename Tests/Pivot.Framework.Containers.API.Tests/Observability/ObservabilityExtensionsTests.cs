using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Containers.API.Observability;

namespace Pivot.Framework.Containers.API.Tests.Observability;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ObservabilityExtensions"/>.
///              Verifies that OpenTelemetry tracing and metrics are registered in DI.
/// </summary>
public class ObservabilityExtensionsTests
{
	[Fact]
	public void AddPivotObservability_ShouldRegisterOpenTelemetryServices()
	{
		var services = new ServiceCollection();
		services.AddLogging();

		services.AddPivotObservability("test-service");

		services.Should().Contain(sd => sd.ServiceType.FullName != null &&
			sd.ServiceType.FullName.Contains("Telemetry", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void AddPivotObservability_WithNullServices_ShouldThrow()
	{
		IServiceCollection services = null!;

		var act = () => services.AddPivotObservability("test");

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void AddPivotObservability_WithNullServiceName_ShouldThrow()
	{
		var services = new ServiceCollection();

		var act = () => services.AddPivotObservability(null!);

		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void AddPivotObservability_WithEmptyServiceName_ShouldThrow()
	{
		var services = new ServiceCollection();

		var act = () => services.AddPivotObservability("");

		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void AddPivotObservability_WithOtlpEndpoint_ShouldNotThrow()
	{
		var services = new ServiceCollection();
		services.AddLogging();

		var act = () => services.AddPivotObservability("test-service", "http://localhost:4317");

		act.Should().NotThrow();
	}

	[Fact]
	public void AddPivotObservability_ShouldReturnSameCollection()
	{
		var services = new ServiceCollection();
		services.AddLogging();

		var result = services.AddPivotObservability("test-service");

		result.Should().BeSameAs(services);
	}
}
