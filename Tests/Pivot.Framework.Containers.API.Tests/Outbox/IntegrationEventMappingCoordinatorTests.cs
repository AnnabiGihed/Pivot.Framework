using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.IntegrationEventMapping;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventMappingCoordinator{TContext}"/>.
///              Verifies that mapped integration events are published and that failures
///              are propagated without hiding mapper or publisher errors.
/// </summary>
public class IntegrationEventMappingCoordinatorTests
{
	#region Test Infrastructure

	public sealed class TestContext : IPersistenceContext
	{
	}

	private sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc, string Name) : IDomainEvent;
	private sealed record AnotherDomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;
	private sealed record TestIntegrationEvent(Guid Id, DateTime OccurredOnUtc, string? CorrelationId, string Name) : IIntegrationEvent;

	private sealed class SingleMapper : IIntegrationEventMapper<TestDomainEvent>
	{
		public IEnumerable<IIntegrationEvent> Map(TestDomainEvent domainEvent)
		{
			yield return new TestIntegrationEvent(Guid.NewGuid(), domainEvent.OccurredOnUtc, "corr-1", $"{domainEvent.Name}-1");
			yield return new TestIntegrationEvent(Guid.NewGuid(), domainEvent.OccurredOnUtc, "corr-2", $"{domainEvent.Name}-2");
		}
	}

	private sealed class SecondaryMapper : IIntegrationEventMapper<TestDomainEvent>
	{
		public IEnumerable<IIntegrationEvent> Map(TestDomainEvent domainEvent)
		{
			yield return new TestIntegrationEvent(Guid.NewGuid(), domainEvent.OccurredOnUtc, "corr-3", $"{domainEvent.Name}-3");
		}
	}

	#endregion

	[Fact]
	public async Task PublishMappedIntegrationEventsAsync_PublishesAllMappedEvents()
	{
		var publisher = Substitute.For<IIntegrationEventPublisher<TestContext>>();
		publisher.PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var services = new ServiceCollection();
		services.AddSingleton(publisher);
		services.AddSingleton<IIntegrationEventMapper<TestDomainEvent>, SingleMapper>();
		services.AddSingleton<IIntegrationEventMapper<TestDomainEvent>, SecondaryMapper>();

		using var provider = services.BuildServiceProvider();
		var coordinator = new IntegrationEventMappingCoordinator<TestContext>(provider, publisher);
		var domainEvent = new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow, "schema.activated");

		var result = await coordinator.PublishMappedIntegrationEventsAsync([domainEvent], CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await publisher.Received(3).PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task PublishMappedIntegrationEventsAsync_ReturnsFailure_WhenIntegrationPublishFails()
	{
		var publisher = Substitute.For<IIntegrationEventPublisher<TestContext>>();
		publisher.PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>())
			.Returns(Result.Failure(new Error("Broker.Error", "publish failed")));

		var services = new ServiceCollection();
		services.AddSingleton(publisher);
		services.AddSingleton<IIntegrationEventMapper<TestDomainEvent>, SingleMapper>();

		using var provider = services.BuildServiceProvider();
		var coordinator = new IntegrationEventMappingCoordinator<TestContext>(provider, publisher);
		var domainEvent = new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow, "schema.activated");

		var result = await coordinator.PublishMappedIntegrationEventsAsync([domainEvent], CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		result.Error!.Code.Should().Be("Broker.Error");
	}

	[Fact]
	public async Task PublishMappedIntegrationEventsAsync_NoMapperRegistered_ShouldReturnSuccess()
	{
		var publisher = Substitute.For<IIntegrationEventPublisher<TestContext>>();

		var services = new ServiceCollection();
		services.AddSingleton(publisher);

		using var provider = services.BuildServiceProvider();
		var coordinator = new IntegrationEventMappingCoordinator<TestContext>(provider, publisher);
		var domainEvent = new AnotherDomainEvent(Guid.NewGuid(), DateTime.UtcNow);

		var result = await coordinator.PublishMappedIntegrationEventsAsync([domainEvent], CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await publisher.DidNotReceive().PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
	}
}
