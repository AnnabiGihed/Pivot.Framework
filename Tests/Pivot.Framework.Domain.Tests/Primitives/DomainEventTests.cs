using FluentAssertions;
using Pivot.Framework.Domain.DomainEvents;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="DomainEvent"/>.
///              Verifies auto-generation of Id and OccurredOnUtc, and explicit constructor.
/// </summary>
public class DomainEventTests
{
	#region Default Constructor Tests
	/// <summary>
	/// Verifies that the parameterless constructor generates a unique Id.
	/// </summary>
	[Fact]
	public void DefaultConstructor_ShouldGenerateUniqueId()
	{
		var event1 = new TestDomainEvent("event1");
		var event2 = new TestDomainEvent("event2");

		event1.Id.Should().NotBe(Guid.Empty);
		event2.Id.Should().NotBe(Guid.Empty);
		event1.Id.Should().NotBe(event2.Id);
	}

	/// <summary>
	/// Verifies that the parameterless constructor sets OccurredOnUtc to approximately now.
	/// </summary>
	[Fact]
	public void DefaultConstructor_ShouldSetOccurredOnUtc()
	{
		var before = DateTime.UtcNow;
		var domainEvent = new TestDomainEvent("test");
		var after = DateTime.UtcNow;

		domainEvent.OccurredOnUtc.Should().BeOnOrAfter(before);
		domainEvent.OccurredOnUtc.Should().BeOnOrBefore(after);
	}
	#endregion

	#region IDomainEvent Tests
	/// <summary>
	/// Verifies that <see cref="TestDomainEvent"/> implements <see cref="IDomainEvent"/>.
	/// </summary>
	[Fact]
	public void DomainEvent_ShouldImplementIDomainEvent()
	{
		var domainEvent = new TestDomainEvent("test");

		domainEvent.Should().BeAssignableTo<IDomainEvent>();
	}

	/// <summary>
	/// Verifies that the parameterless constructor creates a valid event.
	/// </summary>
	[Fact]
	public void ParameterlessConstructor_ShouldCreateValidEvent()
	{
		var domainEvent = new TestDomainEvent();

		domainEvent.Id.Should().NotBe(Guid.Empty);
		domainEvent.Description.Should().BeEmpty();
	}
	#endregion

	#region Record Equality Tests
	/// <summary>
	/// Verifies that two events with same Id are equal (record semantics).
	/// </summary>
	[Fact]
	public void RecordEquality_SameIdAndProperties_ShouldBeEqual()
	{
		var id = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var event1 = new TestDomainEvent("test") { Id = id, OccurredOnUtc = now };
		var event2 = new TestDomainEvent("test") { Id = id, OccurredOnUtc = now };

		event1.Should().Be(event2);
	}
	#endregion
}
