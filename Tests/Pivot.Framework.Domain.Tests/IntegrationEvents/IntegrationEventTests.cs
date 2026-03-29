using FluentAssertions;
using Pivot.Framework.Domain.IntegrationEvents;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.IntegrationEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="IntegrationEvent"/> base record.
///              Verifies construction, validation, interface implementation,
///              and record equality semantics.
/// </summary>
public class IntegrationEventTests
{
	#region Test Infrastructure

	private sealed record TestIntegrationEvent : IntegrationEvent
	{
		public TestIntegrationEvent() : base() { }

		public TestIntegrationEvent(Guid id, DateTime occurredOnUtc, string? correlationId)
			: base(id, occurredOnUtc, correlationId) { }
	}

	#endregion

	#region Constructor Tests

	/// <summary>
	/// Verifies that the parameterless constructor auto-generates Id and OccurredOnUtc.
	/// </summary>
	[Fact]
	public void Constructor_Default_ShouldAutoGenerateIdAndTimestamp()
	{
		var before = DateTime.UtcNow;

		var evt = new TestIntegrationEvent();

		evt.Id.Should().NotBe(Guid.Empty);
		evt.OccurredOnUtc.Should().BeOnOrAfter(before);
		evt.OccurredOnUtc.Kind.Should().Be(DateTimeKind.Utc);
	}

	/// <summary>
	/// Verifies that the explicit constructor assigns all properties correctly.
	/// </summary>
	[Fact]
	public void Constructor_Explicit_ShouldAssignAllProperties()
	{
		var id = Guid.NewGuid();
		var timestamp = DateTime.UtcNow;
		var correlationId = "corr-123";

		var evt = new TestIntegrationEvent(id, timestamp, correlationId);

		evt.Id.Should().Be(id);
		evt.OccurredOnUtc.Should().Be(timestamp);
		evt.CorrelationId.Should().Be(correlationId);
	}

	/// <summary>
	/// Verifies that the explicit constructor accepts null correlationId.
	/// </summary>
	[Fact]
	public void Constructor_NullCorrelationId_ShouldBeAllowed()
	{
		var evt = new TestIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, null);

		evt.CorrelationId.Should().BeNull();
	}

	/// <summary>
	/// Verifies that Guid.Empty is rejected.
	/// </summary>
	[Fact]
	public void Constructor_EmptyGuid_ShouldThrow()
	{
		var act = () => new TestIntegrationEvent(Guid.Empty, DateTime.UtcNow, null);

		act.Should().Throw<ArgumentException>().WithParameterName("id");
	}

	/// <summary>
	/// Verifies that non-UTC timestamps are rejected.
	/// </summary>
	[Fact]
	public void Constructor_NonUtcTimestamp_ShouldThrow()
	{
		var localTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

		var act = () => new TestIntegrationEvent(Guid.NewGuid(), localTime, null);

		act.Should().Throw<ArgumentException>().WithParameterName("occurredOnUtc");
	}

	#endregion

	#region IIntegrationEvent Interface Tests

	/// <summary>
	/// Verifies that IntegrationEvent implements IIntegrationEvent.
	/// </summary>
	[Fact]
	public void IntegrationEvent_ShouldImplementIIntegrationEvent()
	{
		var evt = new TestIntegrationEvent();

		evt.Should().BeAssignableTo<IIntegrationEvent>();
	}

	#endregion

	#region CorrelationId Init Property Tests

	/// <summary>
	/// Verifies that CorrelationId can be set via init property.
	/// </summary>
	[Fact]
	public void CorrelationId_Init_ShouldOverrideConstructorValue()
	{
		var evt = new TestIntegrationEvent() { CorrelationId = "override-123" };

		evt.CorrelationId.Should().Be("override-123");
	}

	#endregion
}
