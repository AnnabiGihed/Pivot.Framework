using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Containers.API.Versioning;

namespace Pivot.Framework.Containers.API.Tests.Versioning;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ApiVersioningExtensions"/>.
///              Verifies that API versioning services are registered in the DI container.
/// </summary>
public class ApiVersioningExtensionsTests
{
	[Fact]
	public void AddPivotApiVersioning_ShouldRegisterServices()
	{
		var services = new ServiceCollection();

		services.AddPivotApiVersioning();

		services.Should().Contain(sd => sd.ServiceType.FullName != null &&
			sd.ServiceType.FullName.Contains("ApiVersion"));
	}

	[Fact]
	public void AddPivotApiVersioning_WithNullServices_ShouldThrow()
	{
		IServiceCollection services = null!;

		var act = () => services.AddPivotApiVersioning();

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void AddPivotApiVersioning_ShouldReturnSameServiceCollection()
	{
		var services = new ServiceCollection();

		var result = services.AddPivotApiVersioning();

		result.Should().BeSameAs(services);
	}

	[Fact]
	public void AddPivotApiVersioning_WithCustomVersion_ShouldNotThrow()
	{
		var services = new ServiceCollection();

		var act = () => services.AddPivotApiVersioning(defaultMajorVersion: 2, defaultMinorVersion: 1);

		act.Should().NotThrow();
	}
}
