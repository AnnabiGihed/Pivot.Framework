using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Containers.API.BFF;
using Pivot.Framework.Containers.API.BFF.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.Tests.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BffExtensions"/>.
///              Verifies that BFF infrastructure services are correctly registered in DI.
/// </summary>
public class BffExtensionsTests
{
	[Fact]
	public void AddBffInfrastructure_ShouldRegisterCacheService()
	{
		var services = new ServiceCollection();

		services.AddBffInfrastructure();

		var provider = services.BuildServiceProvider();
		var cacheService = provider.GetService<IBffCacheService>();
		cacheService.Should().NotBeNull();
		cacheService.Should().BeOfType<InMemoryBffCacheService>();
	}

	[Fact]
	public void AddBffInfrastructure_WithConfigure_ShouldApplyOptions()
	{
		var services = new ServiceCollection();

		services.AddBffInfrastructure(options =>
		{
			options.AddCacheable("test-key", TimeSpan.FromSeconds(30));
		});

		var provider = services.BuildServiceProvider();
		var optionsAccessor = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BffCacheOptions>>();
		optionsAccessor.Value.IsCacheable("test-key").Should().BeTrue();
	}

	[Fact]
	public void AddBffInfrastructure_WithNullServices_ShouldThrow()
	{
		IServiceCollection services = null!;

		var act = () => services.AddBffInfrastructure();

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void AddBffInfrastructure_ShouldReturnSameCollection()
	{
		var services = new ServiceCollection();

		var result = services.AddBffInfrastructure();

		result.Should().BeSameAs(services);
	}

	[Fact]
	public void AddBffInfrastructure_CalledTwice_ShouldNotDuplicate()
	{
		var services = new ServiceCollection();

		services.AddBffInfrastructure();
		services.AddBffInfrastructure();

		services.Count(sd => sd.ServiceType == typeof(IBffCacheService)).Should().Be(1);
	}
}
