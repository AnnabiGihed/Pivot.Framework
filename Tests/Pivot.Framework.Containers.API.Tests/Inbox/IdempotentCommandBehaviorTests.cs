using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pivot.Framework.Application.Abstractions.Messaging.Commands;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox.Behaviors;

namespace Pivot.Framework.Containers.API.Tests.Inbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="IdempotentCommandBehavior{TRequest,TResponse}"/>.
///              Verifies that the MediatR pipeline behavior correctly deduplicates
///              commands implementing <see cref="IIdempotentCommand"/> via the inbox pattern.
/// </summary>
public class IdempotentCommandBehaviorTests
{
	#region Test Infrastructure

	private sealed record TestIdempotentCommand(Guid IdempotencyKey) : ICommand, IIdempotentCommand;

	private sealed record TestNonIdempotentCommand : ICommand;

	private readonly IInboxService _inboxService = Substitute.For<IInboxService>();

	private IdempotentCommandBehavior<TestIdempotentCommand, Result> CreateBehavior(IInboxService? inboxService = null)
	{
		return new IdempotentCommandBehavior<TestIdempotentCommand, Result>(
			NullLogger<IdempotentCommandBehavior<TestIdempotentCommand, Result>>.Instance,
			inboxService);
	}

	private static RequestHandlerDelegate<Result> MakeNext(Result result, Action? onCalled = null)
	{
		return (ct) =>
		{
			onCalled?.Invoke();
			return Task.FromResult(result);
		};
	}

	#endregion

	#region Pass-Through Tests

	/// <summary>
	/// Verifies that non-idempotent commands pass through without inbox checks.
	/// </summary>
	[Fact]
	public async Task Handle_NonIdempotentCommand_ShouldPassThrough()
	{
		var behavior = new IdempotentCommandBehavior<TestNonIdempotentCommand, Result>(
			NullLogger<IdempotentCommandBehavior<TestNonIdempotentCommand, Result>>.Instance, _inboxService);
		var command = new TestNonIdempotentCommand();
		var handlerCalled = false;

		RequestHandlerDelegate<Result> next = (ct) =>
		{
			handlerCalled = true;
			return Task.FromResult(Result.Success());
		};

		var result = await behavior.Handle(command, next, CancellationToken.None);

		handlerCalled.Should().BeTrue();
		result.IsSuccess.Should().BeTrue();
		await _inboxService.DidNotReceive().HasBeenProcessedAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that when inbox service is null, the behavior passes through.
	/// </summary>
	[Fact]
	public async Task Handle_NullInboxService_ShouldPassThrough()
	{
		var behavior = CreateBehavior(inboxService: null);
		var command = new TestIdempotentCommand(Guid.NewGuid());
		var handlerCalled = false;

		var next = MakeNext(Result.Success(), () => handlerCalled = true);

		var result = await behavior.Handle(command, next, CancellationToken.None);

		handlerCalled.Should().BeTrue();
		result.IsSuccess.Should().BeTrue();
	}

	#endregion

	#region Deduplication Tests

	/// <summary>
	/// Verifies that an already-processed command short-circuits with success.
	/// </summary>
	[Fact]
	public async Task Handle_AlreadyProcessed_ShouldReturnSuccessWithoutCallingHandler()
	{
		var key = Guid.NewGuid();
		var command = new TestIdempotentCommand(key);
		var behavior = CreateBehavior(_inboxService);
		var handlerCalled = false;

		_inboxService.HasBeenProcessedAsync(key, nameof(TestIdempotentCommand), Arg.Any<CancellationToken>())
			.Returns(true);

		var next = MakeNext(Result.Success(), () => handlerCalled = true);

		var result = await behavior.Handle(command, next, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		handlerCalled.Should().BeFalse();
		await _inboxService.DidNotReceive().RecordConsumptionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that a new command executes the handler and records consumption.
	/// </summary>
	[Fact]
	public async Task Handle_NotYetProcessed_ShouldExecuteHandlerAndRecordConsumption()
	{
		var key = Guid.NewGuid();
		var command = new TestIdempotentCommand(key);
		var behavior = CreateBehavior(_inboxService);

		_inboxService.HasBeenProcessedAsync(key, nameof(TestIdempotentCommand), Arg.Any<CancellationToken>())
			.Returns(false);
		_inboxService.RecordConsumptionAsync(key, nameof(TestIdempotentCommand), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		var next = MakeNext(Result.Success());

		var result = await behavior.Handle(command, next, CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		await _inboxService.Received(1).RecordConsumptionAsync(key, nameof(TestIdempotentCommand), Arg.Any<CancellationToken>());
		await _inboxService.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that a failed handler does NOT record consumption (allows retry).
	/// </summary>
	[Fact]
	public async Task Handle_HandlerFails_ShouldNotRecordConsumption()
	{
		var key = Guid.NewGuid();
		var command = new TestIdempotentCommand(key);
		var behavior = CreateBehavior(_inboxService);

		_inboxService.HasBeenProcessedAsync(key, nameof(TestIdempotentCommand), Arg.Any<CancellationToken>())
			.Returns(false);

		var failureResult = Result.Failure(new Error("Test.Failure", "Handler failed"));
		var next = MakeNext(failureResult);

		var result = await behavior.Handle(command, next, CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		await _inboxService.DidNotReceive().RecordConsumptionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
		await _inboxService.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
	}

	#endregion
}
