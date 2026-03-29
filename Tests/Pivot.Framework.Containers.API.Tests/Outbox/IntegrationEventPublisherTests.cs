using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Application.Abstractions.Correlation;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventPublisher{TContext}"/>.
///              Verifies that integration events are correctly serialized and persisted
///              to the outbox with MessageKind.IntegrationEvent and correlation ID stamping.
/// </summary>
public class IntegrationEventPublisherTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	private sealed record TestIntegrationEvent : IntegrationEvent
	{
		public string OrderId { get; init; } = string.Empty;

		public TestIntegrationEvent() : base() { }
		public TestIntegrationEvent(Guid id, DateTime occurredOnUtc, string? correlationId)
			: base(id, occurredOnUtc, correlationId) { }
	}

	private readonly IOutboxRepository<TestDbContext> _outboxRepository = Substitute.For<IOutboxRepository<TestDbContext>>();
	private readonly ILogger<IntegrationEventPublisher<TestDbContext>> _logger = Substitute.For<ILogger<IntegrationEventPublisher<TestDbContext>>>();

	private IntegrationEventPublisher<TestDbContext> CreatePublisher()
		=> new(_outboxRepository, _logger);

	#endregion

	#region Setup / Teardown

	public IntegrationEventPublisherTests()
	{
		CorrelationContext.CorrelationId = null;
	}

	#endregion

	#region PublishAsync Tests

	/// <summary>
	/// Verifies that an integration event is persisted with correct MessageKind.
	/// </summary>
	[Fact]
	public async Task PublishAsync_ShouldPersistWithIntegrationEventKind()
	{
		OutboxMessage? captured = null;
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				captured = ci.Arg<OutboxMessage>();
				return Result.Success();
			});

		var publisher = CreatePublisher();
		var evt = new TestIntegrationEvent { OrderId = "ORD-123" };

		var result = await publisher.PublishAsync(evt, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		captured.Should().NotBeNull();
		captured!.Kind.Should().Be(MessageKind.IntegrationEvent);
		captured.Id.Should().Be(evt.Id);
		captured.Payload.Should().Contain("ORD-123");
		captured.EventType.Should().Contain(nameof(TestIntegrationEvent));
	}

	/// <summary>
	/// Verifies that correlation ID from the event is used.
	/// </summary>
	[Fact]
	public async Task PublishAsync_EventWithCorrelationId_ShouldUseEventCorrelationId()
	{
		OutboxMessage? captured = null;
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				captured = ci.Arg<OutboxMessage>();
				return Result.Success();
			});

		var publisher = CreatePublisher();
		var evt = new TestIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, "event-corr-456");

		await publisher.PublishAsync(evt, CancellationToken.None);

		captured.Should().NotBeNull();
		captured!.CorrelationId.Should().Be("event-corr-456");
	}

	/// <summary>
	/// Verifies that ambient CorrelationContext is used when event has no correlation ID.
	/// </summary>
	[Fact]
	public async Task PublishAsync_NoEventCorrelationId_ShouldFallBackToAmbientContext()
	{
		OutboxMessage? captured = null;
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				captured = ci.Arg<OutboxMessage>();
				return Result.Success();
			});

		CorrelationContext.CorrelationId = "ambient-corr-789";
		var publisher = CreatePublisher();
		var evt = new TestIntegrationEvent(); // no correlation ID

		await publisher.PublishAsync(evt, CancellationToken.None);

		captured.Should().NotBeNull();
		captured!.CorrelationId.Should().Be("ambient-corr-789");
	}

	/// <summary>
	/// Verifies that null event returns a failure result.
	/// </summary>
	[Fact]
	public async Task PublishAsync_NullEvent_ShouldReturnFailure()
	{
		var publisher = CreatePublisher();

		var result = await publisher.PublishAsync(null!, CancellationToken.None);

		result.IsFailure.Should().BeTrue();
	}

	/// <summary>
	/// Verifies that repository failure is propagated.
	/// </summary>
	[Fact]
	public async Task PublishAsync_RepositoryFails_ShouldReturnFailure()
	{
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(Result.Failure(new Error("DB.Error", "DB write failed")));

		var publisher = CreatePublisher();
		var evt = new TestIntegrationEvent();

		var result = await publisher.PublishAsync(evt, CancellationToken.None);

		result.IsFailure.Should().BeTrue();
	}

	#endregion
}
