using FluentAssertions;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Application.IntegrationEventHandler;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Application.Tests.DomainEventHandler;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventHandlerBase{TEvent}"/>.
///              Verifies notification unwrapping, null guard, and result forwarding.
/// </summary>
public class IntegrationEventHandlerBaseTests
{
	#region Test Infrastructure
	/// <summary>
	/// Test integration event used by the handler tests.
	/// </summary>
	/// <param name="Message">The test message.</param>
	private sealed record TestIntegrationEvent(string Message) : IntegrationEvent;

	/// <summary>
	/// Test handler that records the received integration event.
	/// </summary>
	private sealed class TestHandler : IntegrationEventHandlerBase<TestIntegrationEvent>
	{
		/// <summary>
		/// Gets the integration event received by the handler.
		/// </summary>
		public TestIntegrationEvent? ReceivedEvent { get; private set; }

		/// <summary>
		/// Gets or sets the result returned by the handler.
		/// </summary>
		public Result ResultToReturn { get; set; } = Result.Success();

		/// <inheritdoc />
		public override Task<Result> HandleWithResultAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken)
		{
			ReceivedEvent = integrationEvent;
			return Task.FromResult(ResultToReturn);
		}
	}
	#endregion

	#region Handle Tests
	/// <summary>
	/// Verifies that Handle unwraps the notification and forwards the integration event.
	/// </summary>
	[Fact]
	public async Task Handle_ShouldUnwrapAndForwardEvent()
	{
		var handler = new TestHandler();
		var integrationEvent = new TestIntegrationEvent("Test message");
		var notification = new IntegrationEventNotification<TestIntegrationEvent>(integrationEvent);

		IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandler = handler;
		await integrationEventHandler.Handle(notification, CancellationToken.None);

		handler.ReceivedEvent.Should().Be(integrationEvent);
		handler.ReceivedEvent!.Message.Should().Be("Test message");
	}

	/// <summary>
	/// Verifies that a null notification throws.
	/// </summary>
	[Fact]
	public async Task Handle_NullNotification_ShouldThrow()
	{
		var handler = new TestHandler();
		IIntegrationEventHandler<TestIntegrationEvent> integrationEventHandler = handler;

		var act = () => integrationEventHandler.Handle(null!, CancellationToken.None);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that handler failures are preserved when calling the result-returning handler method directly.
	/// </summary>
	[Fact]
	public async Task HandleWithResultAsync_WhenFailure_ShouldReturnFailure()
	{
		var handler = new TestHandler { ResultToReturn = Result.Failure(new Error("ERR", "test failure")) };
		var integrationEvent = new TestIntegrationEvent("Failure event");

		var result = await handler.HandleWithResultAsync(integrationEvent, CancellationToken.None);

		result.IsFailure.Should().BeTrue();
		result.Error.Code.Should().Be("ERR");
	}

	/// <summary>
	/// Verifies that the handler implements the integration event handler contract.
	/// </summary>
	[Fact]
	public void Handler_ShouldImplementIIntegrationEventHandler()
	{
		var handler = new TestHandler();

		handler.Should().BeAssignableTo<IIntegrationEventHandler<TestIntegrationEvent>>();
	}
	#endregion
}
