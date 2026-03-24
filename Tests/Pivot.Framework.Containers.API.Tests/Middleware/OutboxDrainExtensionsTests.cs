using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Outbox.Extensions;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxDrainExtensions"/>.
///              Verifies exclusive drain mode registration, outbox processor registration,
///              and hosted service registration rules for background polling mode.
/// </summary>
public class OutboxDrainExtensionsTests
{
    #region Test Infrastructure
    public class TestDbContext : DbContext, IPersistenceContext
    {
        public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
        { }
    }
    #endregion

    #region Registration Tests
    /// <summary>
    /// Verifies that immediate mode registers the marker and the scoped outbox processor,
    /// without registering any hosted service.
    /// </summary>
    [Fact]
    public void AddOutboxDraining_ImmediateAfterRequest_ShouldRegisterMarkerAndProcessorOnly()
    {
        var services = new ServiceCollection();

        services.AddOutboxDraining<TestDbContext>(options =>
        {
            options.Mode = OutboxDrainMode.ImmediateAfterRequest;
        });

        var markerDescriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(OutboxDrainRegistrationMarker));

        markerDescriptor.Should().NotBeNull();
        markerDescriptor!.ImplementationInstance.Should().BeOfType<OutboxDrainRegistrationMarker>();

        var marker = (OutboxDrainRegistrationMarker)markerDescriptor.ImplementationInstance!;
        marker.Mode.Should().Be(OutboxDrainMode.ImmediateAfterRequest);

        var processorDescriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(IOutboxProcessor<TestDbContext>));

        processorDescriptor.Should().NotBeNull();
        processorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.Should().NotContain(x => x.ServiceType == typeof(IHostedService));
    }

    /// <summary>
    /// Verifies that background polling mode registers the marker, the scoped outbox processor,
    /// and one hosted service for background draining.
    /// </summary>
    [Fact]
    public void AddOutboxDraining_BackgroundPolling_ShouldRegisterMarkerProcessorAndHostedService()
    {
        var services = new ServiceCollection();

        services.AddOutboxDraining<TestDbContext>(options =>
        {
            options.Mode = OutboxDrainMode.BackgroundPolling;
            options.PollingInterval = TimeSpan.FromSeconds(12);
        });

        var markerDescriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(OutboxDrainRegistrationMarker));

        markerDescriptor.Should().NotBeNull();
        var marker = (OutboxDrainRegistrationMarker)markerDescriptor!.ImplementationInstance!;
        marker.Mode.Should().Be(OutboxDrainMode.BackgroundPolling);

        var processorDescriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(IOutboxProcessor<TestDbContext>));

        processorDescriptor.Should().NotBeNull();
        processorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        services.Should().ContainSingle(x => x.ServiceType == typeof(IHostedService));
    }

    /// <summary>
    /// Verifies that a second drain mode registration throws and therefore prevents
    /// multiple outbox drain modes from being configured at the same time.
    /// </summary>
    [Fact]
    public void AddOutboxDraining_WhenCalledTwice_ShouldThrow()
    {
        var services = new ServiceCollection();

        services.AddOutboxDraining<TestDbContext>(options =>
        {
            options.Mode = OutboxDrainMode.ImmediateAfterRequest;
        });

        var act = () => services.AddOutboxDraining<TestDbContext>(options =>
        {
            options.Mode = OutboxDrainMode.BackgroundPolling;
        });

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Only one outbox draining mode can be configured*");
    }

    /// <summary>
    /// Verifies that null service collection throws.
    /// </summary>
    [Fact]
    public void AddOutboxDraining_NullServices_ShouldThrow()
    {
        ServiceCollection? services = null;

        var act = () => services!.AddOutboxDraining<TestDbContext>(options =>
        {
            options.Mode = OutboxDrainMode.ImmediateAfterRequest;
        });

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that null configure delegate throws.
    /// </summary>
    [Fact]
    public void AddOutboxDraining_NullConfigure_ShouldThrow()
    {
        var services = new ServiceCollection();

        var act = () => services.AddOutboxDraining<TestDbContext>(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    #endregion
}