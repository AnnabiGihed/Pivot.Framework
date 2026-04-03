using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.IntegrationEvents.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventMappingExtensions"/>.
///              Verifies that the mapping coordinator and integration event publisher
///              are registered together for the targeted persistence context.
/// </summary>
public class IntegrationEventMappingExtensionsTests
{
	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	[Fact]
	public void AddIntegrationEventMapping_ShouldRegisterCoordinator()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventMapping<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IIntegrationEventMappingCoordinator<TestDbContext>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	[Fact]
	public void AddIntegrationEventMapping_ShouldAlsoRegisterIntegrationEventPublisher()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventMapping<TestDbContext>();

		services.Should().Contain(x => x.ServiceType == typeof(IIntegrationEventPublisher<TestDbContext>));
		services.Should().Contain(x => x.ServiceType == typeof(IIntegrationEventPublisher));
	}

	[Fact]
	public void AddIntegrationEventMapping_CalledTwice_ShouldNotDuplicateRegistrations()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventMapping<TestDbContext>();
		services.AddIntegrationEventMapping<TestDbContext>();

		services.Count(x => x.ServiceType == typeof(IIntegrationEventMappingCoordinator<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IIntegrationEventPublisher<TestDbContext>)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IIntegrationEventPublisher)).Should().Be(1);
	}
}
