using FluentAssertions;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.IntegrationEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxMessageFailedEvent"/>.
///              Verifies construction, property assignment, and inheritance.
/// </summary>
public class OutboxMessageFailedEventTests
{
	#region Constructor Tests

	/// <summary>
	/// Verifies that the parameterized constructor assigns all properties correctly.
	/// </summary>
	[Fact]
	public void Constructor_WithParameters_ShouldAssignAllProperties()
	{
		var failedId = Guid.NewGuid();
		var correlationId = "corr-456";

		var evt = new OutboxMessageFailedEvent(
			failedMessageId: failedId,
			eventType: "OrderCreatedEvent",
			retryCount: 5,
			lastError: "Connection refused",
			correlationId: correlationId);

		evt.FailedMessageId.Should().Be(failedId);
		evt.OriginalEventType.Should().Be("OrderCreatedEvent");
		evt.RetryCount.Should().Be(5);
		evt.LastError.Should().Be("Connection refused");
		evt.CorrelationId.Should().Be(correlationId);
		evt.Id.Should().NotBe(Guid.Empty);
		evt.OccurredOnUtc.Kind.Should().Be(DateTimeKind.Utc);
	}

	/// <summary>
	/// Verifies that the parameterless constructor creates a valid event.
	/// </summary>
	[Fact]
	public void Constructor_Default_ShouldCreateValidEvent()
	{
		var evt = new OutboxMessageFailedEvent();

		evt.Id.Should().NotBe(Guid.Empty);
		evt.FailedMessageId.Should().Be(Guid.Empty);
		evt.OriginalEventType.Should().BeEmpty();
		evt.RetryCount.Should().Be(0);
		evt.LastError.Should().BeNull();
	}

	#endregion

	#region Inheritance Tests

	/// <summary>
	/// Verifies that OutboxMessageFailedEvent inherits from IntegrationEvent.
	/// </summary>
	[Fact]
	public void OutboxMessageFailedEvent_ShouldInheritFromIntegrationEvent()
	{
		var evt = new OutboxMessageFailedEvent();

		evt.Should().BeAssignableTo<IntegrationEvent>();
		evt.Should().BeAssignableTo<IIntegrationEvent>();
	}

	#endregion
}
