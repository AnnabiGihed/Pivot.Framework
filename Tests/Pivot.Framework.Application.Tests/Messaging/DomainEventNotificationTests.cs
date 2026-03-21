using FluentAssertions;
using Pivot.Framework.Application.Abstractions.Messaging.Events;
using Pivot.Framework.Domain.DomainEvents;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Tests.Messaging;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="DomainEventNotification{TDomainEvent}"/>.
///              Verifies wrapping, null guard, and property access.
/// </summary>
public class DomainEventNotificationTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that the constructor wraps the domain event correctly.
	/// </summary>
	[Fact]
	public void Constructor_ShouldWrapDomainEvent()
	{
		var domainEvent = new TestEvent("Something happened");

		var notification = new DomainEventNotification<TestEvent>(domainEvent);

		notification.DomainEvent.Should().Be(domainEvent);
		notification.DomainEvent.Description.Should().Be("Something happened");
	}

	/// <summary>
	/// Verifies that null domain event throws.
	/// </summary>
	[Fact]
	public void Constructor_NullDomainEvent_ShouldThrow()
	{
		var act = () => new DomainEventNotification<TestEvent>(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region INotification Tests
	/// <summary>
	/// Verifies that DomainEventNotification implements INotification.
	/// </summary>
	[Fact]
	public void DomainEventNotification_ShouldImplementINotification()
	{
		var notification = new DomainEventNotification<TestEvent>(new TestEvent("test"));

		notification.Should().BeAssignableTo<MediatR.INotification>();
	}
	#endregion

	#region Test Doubles
	/// <summary>
	/// Test domain event for notification tests.
	/// </summary>
	private sealed record TestEvent : DomainEvent
	{
		public string Description { get; init; } = string.Empty;
		public TestEvent(string description) : base() { Description = description; }
	}
	#endregion
}
