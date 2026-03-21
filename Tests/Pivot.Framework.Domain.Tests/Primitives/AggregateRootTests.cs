using FluentAssertions;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="AggregateRoot{TId}"/>.
///              Verifies domain event raising, clearing via explicit interface implementation,
///              soft-delete, and restore behaviours.
/// </summary>
public class AggregateRootTests
{
	#region RaiseDomainEvent Tests
	/// <summary>
	/// Verifies that raising a domain event adds it to the aggregate's event collection.
	/// </summary>
	[Fact]
	public void RaiseDomainEvent_ShouldAddEvent_ToCollection()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");

		aggregate.ChangeName("New Name");

		aggregate.GetDomainEvents().Should().HaveCount(1);
	}

	/// <summary>
	/// Verifies that <see cref="AggregateRoot{TId}.GetDomainEvents"/> returns a read-only collection.
	/// </summary>
	[Fact]
	public void GetDomainEvents_ShouldReturnReadOnlyCollection()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");
		aggregate.ChangeName("New Name");

		var events = aggregate.GetDomainEvents();

		events.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
	}

	/// <summary>
	/// Verifies that raising a domain event with a null value throws <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void RaiseDomainEvent_WithNull_ShouldThrowArgumentNullException()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");

		var act = () => aggregate.RaiseTestEvent(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region ClearDomainEvents Tests
	/// <summary>
	/// Verifies that <see cref="IAggregateRoot.ClearDomainEvents"/> is not callable
	/// on the concrete type — only through the <see cref="IAggregateRoot"/> interface.
	/// </summary>
	[Fact]
	public void ClearDomainEvents_ShouldNotBeCallableOnConcreteType()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");
		aggregate.ChangeName("New Name");

		// Can only call through the interface
		var asInterface = (IAggregateRoot)aggregate;
		asInterface.ClearDomainEvents();

		aggregate.GetDomainEvents().Should().BeEmpty();
	}

	/// <summary>
	/// Verifies that clearing domain events via the <see cref="IAggregateRoot"/> interface
	/// removes all collected events.
	/// </summary>
	[Fact]
	public void ClearDomainEvents_ViaInterface_ShouldClearAllEvents()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");
		aggregate.ChangeName("Name1");
		aggregate.ChangeName("Name2");

		aggregate.GetDomainEvents().Should().HaveCount(2);

		((IAggregateRoot)aggregate).ClearDomainEvents();

		aggregate.GetDomainEvents().Should().BeEmpty();
	}
	#endregion

	#region Soft-Delete Tests
	/// <summary>
	/// Verifies that calling <see cref="AggregateRoot{TId}.Delete"/> marks the aggregate
	/// as soft-deleted with the correct metadata.
	/// </summary>
	[Fact]
	public void Delete_ShouldSoftDeleteAggregate()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		aggregate.PerformDelete(now, "admin");

		aggregate.IsDeleted.Should().BeTrue();
		aggregate.DeletedBy.Should().Be("admin");
		aggregate.DeletedOnUtc.Should().Be(now);
	}

	/// <summary>
	/// Verifies that calling <see cref="AggregateRoot{TId}.RestoreDeleted"/> undoes a
	/// previous soft-delete and clears all deletion metadata.
	/// </summary>
	[Fact]
	public void RestoreDeleted_ShouldUndoSoftDelete()
	{
		var aggregate = new TestAggregateRoot(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		aggregate.PerformDelete(now, "admin");
		aggregate.PerformRestore(now.AddMinutes(5), "admin");

		aggregate.IsDeleted.Should().BeFalse();
		aggregate.DeletedBy.Should().BeNull();
		aggregate.DeletedOnUtc.Should().BeNull();
	}
	#endregion
}
