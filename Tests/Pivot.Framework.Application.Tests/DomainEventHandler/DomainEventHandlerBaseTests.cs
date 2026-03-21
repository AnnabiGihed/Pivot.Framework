using FluentAssertions;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Application.DomainEventHandler;
using Pivot.Framework.Domain.DomainEvents;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Tests.DomainEventHandler;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="DomainEventHandlerBase{TEvent}"/>.
///              Verifies notification unwrapping, null guard, and result forwarding.
/// </summary>
public class DomainEventHandlerBaseTests
{
	#region Test Infrastructure
	private sealed record TestDomainEvent(string Message) : DomainEvent;

	private sealed class TestHandler : DomainEventHandlerBase<TestDomainEvent>
	{
		public TestDomainEvent? ReceivedEvent { get; private set; }
		public Result ResultToReturn { get; set; } = Result.Success();

		public override Task<Result> HandleWithResultAsync(
			TestDomainEvent domainEvent, CancellationToken cancellationToken)
		{
			ReceivedEvent = domainEvent;
			return Task.FromResult(ResultToReturn);
		}
	}
	#endregion

	#region Handle Tests
	/// <summary>
	/// Verifies that Handle unwraps the notification and forwards the domain event.
	/// </summary>
	[Fact]
	public async Task Handle_ShouldUnwrapAndForwardEvent()
	{
		var handler = new TestHandler();
		var domainEvent = new TestDomainEvent("Test message");
		var notification = new DomainEventNotification<TestDomainEvent>(domainEvent);

		await handler.Handle(notification, CancellationToken.None);

		handler.ReceivedEvent.Should().Be(domainEvent);
		handler.ReceivedEvent!.Message.Should().Be("Test message");
	}

	/// <summary>
	/// Verifies that null notification throws.
	/// </summary>
	[Fact]
	public async Task Handle_NullNotification_ShouldThrow()
	{
		var handler = new TestHandler();

		var act = () => handler.Handle(null!, CancellationToken.None);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that the handler implements IDomainEventHandler.
	/// </summary>
	[Fact]
	public void Handler_ShouldImplementIDomainEventHandler()
	{
		var handler = new TestHandler();

		handler.Should().BeAssignableTo<IDomainEventHandler<TestDomainEvent>>();
	}
	#endregion
}
