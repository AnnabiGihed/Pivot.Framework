using FluentAssertions;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Application.Tests.Messaging;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="IntegrationEventNotification{TIntegrationEvent}"/>.
///              Verifies wrapping, null guard, and MediatR notification compatibility.
/// </summary>
public class IntegrationEventNotificationTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that the constructor wraps the integration event correctly.
	/// </summary>
	[Fact]
	public void Constructor_ShouldWrapIntegrationEvent()
	{
		var integrationEvent = new TestIntegrationEvent("Something happened");

		var notification = new IntegrationEventNotification<TestIntegrationEvent>(integrationEvent);

		notification.IntegrationEvent.Should().Be(integrationEvent);
		notification.IntegrationEvent.Description.Should().Be("Something happened");
	}

	/// <summary>
	/// Verifies that a null integration event throws.
	/// </summary>
	[Fact]
	public void Constructor_NullIntegrationEvent_ShouldThrow()
	{
		var act = () => new IntegrationEventNotification<TestIntegrationEvent>(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region INotification Tests
	/// <summary>
	/// Verifies that the integration event notification implements MediatR notification.
	/// </summary>
	[Fact]
	public void IntegrationEventNotification_ShouldImplementINotification()
	{
		var notification = new IntegrationEventNotification<TestIntegrationEvent>(new TestIntegrationEvent("test"));

		notification.Should().BeAssignableTo<MediatR.INotification>();
	}
	#endregion

	#region Test Doubles
	/// <summary>
	/// Test integration event used for notification tests.
	/// </summary>
	private sealed record TestIntegrationEvent(string Description) : IntegrationEvent;
	#endregion
}
