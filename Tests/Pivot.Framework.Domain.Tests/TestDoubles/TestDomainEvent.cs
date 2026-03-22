using Pivot.Framework.Domain.DomainEvents;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for a domain event.
///              Carries a simple description for assertion purposes in unit tests.
/// </summary>
public sealed record TestDomainEvent : DomainEvent
{
	#region Properties
	/// <summary>
	/// Gets a human-readable description of what occurred in this test event.
	/// </summary>
	public string Description { get; init; } = string.Empty;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="TestDomainEvent"/> with the specified description.
	/// </summary>
	/// <param name="description">A description of the test event.</param>
	public TestDomainEvent(string description) : base()
	{
		Description = description;
	}

	/// <summary>
	/// Initialises a new <see cref="TestDomainEvent"/> with explicit id, timestamp, and description.
	/// Used for record equality tests where deterministic values are needed.
	/// </summary>
	public TestDomainEvent(Guid id, DateTime occurredOnUtc, string description) : base(id, occurredOnUtc)
	{
		Description = description;
	}

	/// <summary>
	/// Parameterless constructor for serialization and test scenarios.
	/// </summary>
	public TestDomainEvent() : base() { }
	#endregion
}
