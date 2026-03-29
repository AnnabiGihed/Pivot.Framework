using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Processor;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxProcessor{TContext}"/> retry and dead-letter behaviour.
///              Verifies that messages exceeding the max retry threshold are dead-lettered,
///              and that an OutboxMessageFailedEvent is optionally emitted.
/// </summary>
public class OutboxProcessorRetryTests
{
	#region Test Infrastructure

	public class TestDbContext : DbContext, IPersistenceContext
	{
		public TestDbContext() : base(new DbContextOptionsBuilder<TestDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
		{ }
	}

	private readonly IOutboxRepository<TestDbContext> _outboxRepository = Substitute.For<IOutboxRepository<TestDbContext>>();
	private readonly IMessagePublisher _messagePublisher = Substitute.For<IMessagePublisher>();
	private readonly ILogger<OutboxProcessor<TestDbContext>> _logger = Substitute.For<ILogger<OutboxProcessor<TestDbContext>>>();

	private OutboxProcessor<TestDbContext> CreateProcessor(OutboxRetryOptions? retryOptions = null)
	{
		var options = Options.Create(retryOptions ?? new OutboxRetryOptions());
		var dbContext = new TestDbContext();
		return new OutboxProcessor<TestDbContext>(_outboxRepository, _messagePublisher, dbContext, _logger, options);
	}

	#endregion

	#region Happy Path Tests

	/// <summary>
	/// Verifies that when no messages are pending, processing returns success.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_NoMessages_ShouldReturnSuccess()
	{
		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage>());

		var processor = CreateProcessor();
		var result = await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
	}

	/// <summary>
	/// Verifies that a successful publish marks the message as processed.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_PublishSucceeds_ShouldMarkAsProcessed()
	{
		var message = new OutboxMessage { Id = Guid.NewGuid(), Payload = "{}", EventType = "Test" };
		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message).Returns(Result.Success());
		_outboxRepository.MarkAsProcessedAsync(message.Id, Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var processor = CreateProcessor();
		var result = await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await _outboxRepository.Received(1).MarkAsProcessedAsync(message.Id, Arg.Any<CancellationToken>());
	}

	#endregion

	#region Retry Increment Tests

	/// <summary>
	/// Verifies that a failed publish increments the retry count.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_PublishFails_ShouldIncrementRetryCount()
	{
		var message = new OutboxMessage { Id = Guid.NewGuid(), Payload = "{}", EventType = "Test", RetryCount = 0 };
		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message)
			.Returns(Result.Failure(new Error("Broker.Down", "Connection refused")));

		var processor = CreateProcessor(new OutboxRetryOptions { MaxRetryCount = 5 });
		await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		message.RetryCount.Should().Be(1);
		message.LastError.Should().Be("Connection refused");
	}

	#endregion

	#region Dead-Letter Tests

	/// <summary>
	/// Verifies that a message exceeding max retries is dead-lettered (marked Processed + FailedAtUtc set).
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_ExceedsMaxRetry_ShouldDeadLetter()
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "Test",
			RetryCount = 4 // Will become 5 after increment, matching MaxRetryCount
		};

		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message)
			.Returns(Result.Failure(new Error("Broker.Down", "Connection refused")));
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var processor = CreateProcessor(new OutboxRetryOptions { MaxRetryCount = 5, EmitFailureEvent = true });
		await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		message.RetryCount.Should().Be(5);
		message.Processed.Should().BeTrue();
		message.FailedAtUtc.Should().NotBeNull();
	}

	/// <summary>
	/// Verifies that when EmitFailureEvent is true, an OutboxMessageFailedEvent is added to the outbox.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_DeadLetter_WithEmitFailureEvent_ShouldAddFailureEventToOutbox()
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "OrderCreated",
			RetryCount = 4,
			CorrelationId = "corr-dead-letter"
		};

		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message)
			.Returns(Result.Failure(new Error("Broker.Down", "Timeout")));

		OutboxMessage? failureMessage = null;
		_outboxRepository.AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				failureMessage = ci.Arg<OutboxMessage>();
				return Result.Success();
			});

		var processor = CreateProcessor(new OutboxRetryOptions { MaxRetryCount = 5, EmitFailureEvent = true });
		await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		failureMessage.Should().NotBeNull();
		failureMessage!.Kind.Should().Be(MessageKind.IntegrationEvent);
		failureMessage.CorrelationId.Should().Be("corr-dead-letter");
		failureMessage.Payload.Should().Contain("OrderCreated");
	}

	/// <summary>
	/// Verifies that when EmitFailureEvent is false, no failure event is added.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_DeadLetter_WithoutEmitFailureEvent_ShouldNotAddFailureEvent()
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "Test",
			RetryCount = 4
		};

		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message)
			.Returns(Result.Failure(new Error("Broker.Down", "Timeout")));

		var processor = CreateProcessor(new OutboxRetryOptions { MaxRetryCount = 5, EmitFailureEvent = false });
		await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		message.Processed.Should().BeTrue();
		message.FailedAtUtc.Should().NotBeNull();
		await _outboxRepository.DidNotReceive().AddAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that a message below the threshold is NOT dead-lettered.
	/// </summary>
	[Fact]
	public async Task ProcessOutboxMessagesAsync_BelowThreshold_ShouldNotDeadLetter()
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "Test",
			RetryCount = 2 // Will become 3, still below MaxRetryCount of 5
		};

		_outboxRepository.GetUnprocessedMessagesAsync(Arg.Any<CancellationToken>())
			.Returns(new List<OutboxMessage> { message });
		_messagePublisher.PublishAsync(message)
			.Returns(Result.Failure(new Error("Broker.Down", "Timeout")));

		var processor = CreateProcessor(new OutboxRetryOptions { MaxRetryCount = 5 });
		await processor.ProcessOutboxMessagesAsync(CancellationToken.None);

		message.RetryCount.Should().Be(3);
		message.Processed.Should().BeFalse();
		message.FailedAtUtc.Should().BeNull();
	}

	#endregion
}
