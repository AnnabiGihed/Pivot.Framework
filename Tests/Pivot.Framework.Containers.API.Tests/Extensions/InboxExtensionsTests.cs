using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;
using Pivot.Framework.Infrastructure.Abstraction.Inbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Inbox.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="InboxExtensions"/>.
///              Verifies that AddInboxSupport and AddIdempotentCommands register
///              the expected services with correct lifetimes.
/// </summary>
public class InboxExtensionsTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	#endregion

	#region AddInboxSupport Tests

	/// <summary>
	/// Verifies that AddInboxSupport registers IInboxRepository as scoped.
	/// </summary>
	[Fact]
	public void AddInboxSupport_ShouldRegisterInboxRepository()
	{
		var services = new ServiceCollection();

		services.AddInboxSupport<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IInboxRepository<>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that AddInboxSupport registers IInboxService as scoped.
	/// </summary>
	[Fact]
	public void AddInboxSupport_ShouldRegisterInboxService()
	{
		var services = new ServiceCollection();

		services.AddInboxSupport<TestDbContext>();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IInboxService));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
	}

	/// <summary>
	/// Verifies that calling AddInboxSupport twice does not duplicate registrations (TryAdd).
	/// </summary>
	[Fact]
	public void AddInboxSupport_CalledTwice_ShouldNotDuplicate()
	{
		var services = new ServiceCollection();

		services.AddInboxSupport<TestDbContext>();
		services.AddInboxSupport<TestDbContext>();

		services.Count(x => x.ServiceType == typeof(IInboxService)).Should().Be(1);
	}

	#endregion

	#region AddIdempotentCommands Tests

	/// <summary>
	/// Verifies that AddIdempotentCommands registers the open-generic pipeline behavior.
	/// </summary>
	[Fact]
	public void AddIdempotentCommands_ShouldRegisterPipelineBehavior()
	{
		var services = new ServiceCollection();

		services.AddIdempotentCommands();

		var descriptor = services.SingleOrDefault(x =>
			x.ServiceType == typeof(IPipelineBehavior<,>));

		descriptor.Should().NotBeNull();
		descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
	}

	#endregion
}
