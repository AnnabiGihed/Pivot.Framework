using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for an aggregate root.
///              Exposes protected methods (<see cref="AggregateRoot{TId}.RaiseDomainEvent"/>,
///              <see cref="AggregateRoot{TId}.Delete"/>, <see cref="AggregateRoot{TId}.RestoreDeleted"/>)
///              as public for unit test assertions.
/// </summary>
public sealed class TestAggregateRoot : AggregateRoot<TestId>
{
	#region Properties
	/// <summary>
	/// Gets the name of this test aggregate.
	/// </summary>
	public string Name { get; private set; } = string.Empty;
	#endregion

	#region Constructors
	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	private TestAggregateRoot() : base() { }

	/// <summary>
	/// Initialises a new <see cref="TestAggregateRoot"/> with the specified identity and name.
	/// </summary>
	/// <param name="id">The strongly-typed identifier for this aggregate.</param>
	/// <param name="name">The name of this test aggregate.</param>
	public TestAggregateRoot(TestId id, string name) : base(id)
	{
		Name = name;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Changes the name of this aggregate and raises a <see cref="TestDomainEvent"/>.
	/// </summary>
	/// <param name="newName">The new name value.</param>
	public void ChangeName(string newName)
	{
		Name = newName;
		RaiseDomainEvent(new TestDomainEvent($"Name changed to {newName}"));
	}

	/// <summary>
	/// Raises a test domain event. Exposes the protected <see cref="AggregateRoot{TId}.RaiseDomainEvent"/>
	/// for unit test assertions.
	/// </summary>
	/// <param name="domainEvent">The test event to raise.</param>
	public void RaiseTestEvent(TestDomainEvent domainEvent)
	{
		RaiseDomainEvent(domainEvent);
	}

	/// <summary>
	/// Soft-deletes this aggregate. Exposes the protected <see cref="AggregateRoot{TId}.Delete"/>
	/// for unit test assertions.
	/// </summary>
	/// <param name="deletedOnUtc">UTC timestamp of deletion.</param>
	/// <param name="deletedBy">Actor who performed the deletion.</param>
	public void PerformDelete(DateTime deletedOnUtc, string deletedBy)
	{
		Delete(deletedOnUtc, deletedBy);
	}

	/// <summary>
	/// Restores a previously soft-deleted aggregate. Exposes the protected
	/// <see cref="AggregateRoot{TId}.RestoreDeleted"/> for unit test assertions.
	/// </summary>
	/// <param name="restoredOnUtc">UTC timestamp of restoration.</param>
	/// <param name="restoredBy">Actor who performed the restoration.</param>
	public void PerformRestore(DateTime restoredOnUtc, string restoredBy)
	{
		RestoreDeleted(restoredOnUtc, restoredBy);
	}
	#endregion
}
