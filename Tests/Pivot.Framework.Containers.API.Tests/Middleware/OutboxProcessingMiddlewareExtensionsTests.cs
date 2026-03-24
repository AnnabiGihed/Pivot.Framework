using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxProcessingMiddlewareExtensions"/>.
///              Verifies that middleware-based outbox draining is allowed only when
///              the configured drain mode is ImmediateAfterRequest.
/// </summary>
public class OutboxProcessingMiddlewareExtensionsTests
{
    #region Test Infrastructure
    public class TestDbContext : DbContext, IPersistenceContext
    {
        public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
        { }
    }
    #endregion

    #region Guard Tests
    /// <summary>
    /// Verifies that calling the extension without a configured outbox mode throws.
    /// </summary>
    [Fact]
    public void UseImmediateOutboxDraining_NoMarkerRegistered_ShouldThrow()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var act = () => app.UseImmediateOutboxDraining<TestDbContext>();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Outbox draining has not been configured*");
    }

    /// <summary>
    /// Verifies that calling the extension while background polling mode is configured throws.
    /// </summary>
    [Fact]
    public void UseImmediateOutboxDraining_BackgroundPollingConfigured_ShouldThrow()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new OutboxDrainRegistrationMarker(OutboxDrainMode.BackgroundPolling));

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var act = () => app.UseImmediateOutboxDraining<TestDbContext>();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*BackgroundPolling*")
            .WithMessage("*ImmediateAfterRequest*");
    }

    /// <summary>
    /// Verifies that calling the extension with immediate mode configured succeeds
    /// and returns the same application builder instance for fluent chaining.
    /// </summary>
    [Fact]
    public void UseImmediateOutboxDraining_ImmediateAfterRequestConfigured_ShouldReturnApplicationBuilder()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new OutboxDrainRegistrationMarker(OutboxDrainMode.ImmediateAfterRequest));

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var result = app.UseImmediateOutboxDraining<TestDbContext>();

        result.Should().BeSameAs(app);
    }

    /// <summary>
    /// Verifies that null application builder throws.
    /// </summary>
    [Fact]
    public void UseImmediateOutboxDraining_NullApplicationBuilder_ShouldThrow()
    {
        IApplicationBuilder? app = null;

        var act = () => app!.UseImmediateOutboxDraining<TestDbContext>();

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion
}