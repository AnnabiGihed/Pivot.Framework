using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pivot.Framework.Application.Abstractions;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.UnitOfWork;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for integration-event mapping in <see cref="UnitOfWork{TContext}"/>.
///              Verifies that mapped integration events are published during the same
///              save pipeline and that the optional coordinator remains fully opt-in.
/// </summary>
public class UnitOfWorkIntegrationEventMappingTests
{
	#region Test Infrastructure

	public sealed class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
		{
		}

		public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
	}

	private sealed class TestUnitOfWork : UnitOfWork<TestDbContext>
	{
		public TestUnitOfWork(
			TestDbContext dbContext,
			ICurrentUserProvider currentUserProvider,
			IDomainEventPublisher<TestDbContext> domainEventPublisher,
			IIntegrationEventMappingCoordinator<TestDbContext>? integrationEventMappingCoordinator = null)
			: base(dbContext, currentUserProvider, domainEventPublisher, integrationEventMappingCoordinator)
		{
		}
	}

	public sealed class TestAggregate : IAggregateRoot
	{
		private readonly List<IDomainEvent> _domainEvents = [];

		public Guid Id { get; init; } = Guid.NewGuid();
		public int Version { get; private set; }
		public string Name { get; set; } = string.Empty;

		public void Raise(IDomainEvent domainEvent)
		{
			_domainEvents.Add(domainEvent);
			Version++;
		}

		public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
		public void ClearDomainEvents() => _domainEvents.Clear();
	}

	private sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc, string Name) : IDomainEvent;

	private static TestDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new TestDbContext(options);
	}

	#endregion

	[Fact]
	public async Task SaveChangesAsync_PublishesMappedIntegrationEvents_InSameCommitPath()
	{
		await using var dbContext = CreateDbContext();
		var currentUserProvider = Substitute.For<ICurrentUserProvider>();
		currentUserProvider.GetCurrentUser().Returns("system");

		var domainEventPublisher = Substitute.For<IDomainEventPublisher<TestDbContext>>();
		domainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<IAggregateRoot>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var mappingCoordinator = Substitute.For<IIntegrationEventMappingCoordinator<TestDbContext>>();
		mappingCoordinator.PublishMappedIntegrationEventsAsync(Arg.Any<IReadOnlyCollection<IDomainEvent>>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var aggregate = new TestAggregate { Name = "schema" };
		var domainEvent = new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow, "activated");
		aggregate.Raise(domainEvent);
		dbContext.Aggregates.Add(aggregate);

		var unitOfWork = new TestUnitOfWork(dbContext, currentUserProvider, domainEventPublisher, mappingCoordinator);

		var result = await unitOfWork.SaveChangesAsync(CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await domainEventPublisher.Received(1).PublishAsync(domainEvent, aggregate, Arg.Any<CancellationToken>());
		await mappingCoordinator.Received(1).PublishMappedIntegrationEventsAsync(
			Arg.Is<IReadOnlyCollection<IDomainEvent>>(events =>
				events.Count == 1 &&
				events.Single().Id == domainEvent.Id &&
				events.Single().OccurredOnUtc == domainEvent.OccurredOnUtc),
			Arg.Any<CancellationToken>());
		aggregate.GetDomainEvents().Should().BeEmpty();
	}

	[Fact]
	public async Task SaveChangesAsync_SkipsMapping_WhenNoCoordinatorRegistered()
	{
		await using var dbContext = CreateDbContext();
		var currentUserProvider = Substitute.For<ICurrentUserProvider>();
		currentUserProvider.GetCurrentUser().Returns("system");

		var domainEventPublisher = Substitute.For<IDomainEventPublisher<TestDbContext>>();
		domainEventPublisher.PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<IAggregateRoot>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var aggregate = new TestAggregate { Name = "schema" };
		var domainEvent = new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow, "activated");
		aggregate.Raise(domainEvent);
		dbContext.Aggregates.Add(aggregate);

		var unitOfWork = new TestUnitOfWork(dbContext, currentUserProvider, domainEventPublisher);

		var result = await unitOfWork.SaveChangesAsync(CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await domainEventPublisher.Received(1).PublishAsync(domainEvent, aggregate, Arg.Any<CancellationToken>());
		aggregate.GetDomainEvents().Should().BeEmpty();
	}
}
