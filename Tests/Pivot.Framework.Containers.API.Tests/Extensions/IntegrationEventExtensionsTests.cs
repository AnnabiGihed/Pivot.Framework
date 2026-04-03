using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.IntegrationEvents.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventExtensions"/>.
///              Verifies that AddIntegrationEventPublisher registers the expected service.
/// </summary>
public class IntegrationEventExtensionsTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	#endregion

	#region AddIntegrationEventPublisher Tests

	/// <summary>
	/// Verifies that AddIntegrationEventPublisher registers IIntegrationEventPublisher as scoped.
	/// </summary>
	[Fact]
	public void AddIntegrationEventPublisher_ShouldRegisterPublisher()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventPublisher<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IIntegrationEventPublisher));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that AddIntegrationEventPublisher registers the context-specific publisher.
	/// </summary>
	[Fact]
	public void AddIntegrationEventPublisher_ShouldRegisterContextSpecificPublisher()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventPublisher<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IIntegrationEventPublisher<TestDbContext>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that calling AddIntegrationEventPublisher twice does not duplicate registrations.
	/// </summary>
	[Fact]
	public void AddIntegrationEventPublisher_CalledTwice_ShouldNotDuplicate()
	{
		var services = new ServiceCollection();

		services.AddIntegrationEventPublisher<TestDbContext>();
		services.AddIntegrationEventPublisher<TestDbContext>();

		services.Count(x => x.ServiceType == typeof(IIntegrationEventPublisher)).Should().Be(1);
		services.Count(x => x.ServiceType == typeof(IIntegrationEventPublisher<TestDbContext>)).Should().Be(1);
	}

	#endregion
}
