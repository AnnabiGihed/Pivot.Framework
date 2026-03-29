using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxDrainExtensions.ConfigureOutboxRetry"/>.
///              Verifies that retry options can be customized via DI configuration.
/// </summary>
public class OutboxRetryConfigurationTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	#endregion

	#region ConfigureOutboxRetry Tests

	/// <summary>
	/// Verifies that default OutboxRetryOptions are registered by AddOutboxDraining.
	/// </summary>
	[Fact]
	public void AddOutboxDraining_ShouldRegisterDefaultRetryOptions()
	{
		var services = new ServiceCollection();

		services.AddOutboxDraining<TestDbContext>(o => o.Mode = OutboxDrainMode.ImmediateAfterRequest);

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<OutboxRetryOptions>>();

		options.Value.MaxRetryCount.Should().Be(5);
		options.Value.EmitFailureEvent.Should().BeTrue();
	}

	/// <summary>
	/// Verifies that ConfigureOutboxRetry overrides default retry options.
	/// </summary>
	[Fact]
	public void ConfigureOutboxRetry_ShouldOverrideDefaults()
	{
		var services = new ServiceCollection();

		services.AddOutboxDraining<TestDbContext>(o => o.Mode = OutboxDrainMode.ImmediateAfterRequest);
		services.ConfigureOutboxRetry(o =>
		{
			o.MaxRetryCount = 10;
			o.EmitFailureEvent = false;
		});

		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<OutboxRetryOptions>>();

		options.Value.MaxRetryCount.Should().Be(10);
		options.Value.EmitFailureEvent.Should().BeFalse();
	}

	/// <summary>
	/// Verifies that null services throws ArgumentNullException.
	/// </summary>
	[Fact]
	public void ConfigureOutboxRetry_NullServices_ShouldThrow()
	{
		ServiceCollection? services = null;

		var act = () => services!.ConfigureOutboxRetry(o => o.MaxRetryCount = 3);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null configure delegate throws ArgumentNullException.
	/// </summary>
	[Fact]
	public void ConfigureOutboxRetry_NullConfigure_ShouldThrow()
	{
		var services = new ServiceCollection();

		var act = () => services.ConfigureOutboxRetry(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	#endregion
}
