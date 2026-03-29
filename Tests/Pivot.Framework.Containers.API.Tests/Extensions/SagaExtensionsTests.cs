using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Application.Abstractions.Sagas;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Sagas.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SagaExtensions"/>.
///              Verifies that AddSagaSupport registers the expected services.
/// </summary>
public class SagaExtensionsTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	#endregion

	#region AddSagaSupport Tests

	/// <summary>
	/// Verifies that AddSagaSupport registers ISagaRepository as scoped.
	/// </summary>
	[Fact]
	public void AddSagaSupport_ShouldRegisterSagaRepository()
	{
		var services = new ServiceCollection();

		services.AddSagaSupport<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(ISagaRepository<>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that AddSagaSupport registers ISagaOrchestrator as scoped.
	/// </summary>
	[Fact]
	public void AddSagaSupport_ShouldRegisterSagaOrchestrator()
	{
		var services = new ServiceCollection();

		services.AddSagaSupport<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(ISagaOrchestrator));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that calling AddSagaSupport twice does not duplicate registrations.
	/// </summary>
	[Fact]
	public void AddSagaSupport_CalledTwice_ShouldNotDuplicate()
	{
		var services = new ServiceCollection();

		services.AddSagaSupport<TestDbContext>();
		services.AddSagaSupport<TestDbContext>();

		services.Count(x => x.ServiceType == typeof(ISagaOrchestrator)).Should().Be(1);
	}

	#endregion
}
