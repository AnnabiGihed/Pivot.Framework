using FluentAssertions;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="Entity{TId}"/>.
///              Verifies identity-based equality, audit initialisation, soft-delete,
///              restore, and validation guards.
/// </summary>
public class EntityTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that constructing an entity with a valid ID assigns it correctly.
	/// </summary>
	[Fact]
	public void Constructor_WithValidId_ShouldAssignId()
	{
		var id = TestId.New();
		var entity = new TestEntity(id, "Test");

		entity.Id.Should().Be(id);
	}

	/// <summary>
	/// Verifies that constructing an entity with null ID throws.
	/// </summary>
	[Fact]
	public void Constructor_WithNullId_ShouldThrow()
	{
		var act = () => new TestEntity(null!, "Test");

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Equality Tests
	/// <summary>
	/// Verifies that two entities with the same ID are equal.
	/// </summary>
	[Fact]
	public void Equals_SameId_ShouldBeTrue()
	{
		var id = TestId.New();
		var entity1 = new TestEntity(id, "A");
		var entity2 = new TestEntity(id, "B");

		entity1.Equals(entity2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that two entities with different IDs are not equal.
	/// </summary>
	[Fact]
	public void Equals_DifferentId_ShouldBeFalse()
	{
		var entity1 = new TestEntity(TestId.New(), "A");
		var entity2 = new TestEntity(TestId.New(), "B");

		entity1.Equals(entity2).Should().BeFalse();
	}

	/// <summary>
	/// Verifies that comparing to null returns false.
	/// </summary>
	[Fact]
	public void Equals_Null_ShouldBeFalse()
	{
		var entity = new TestEntity(TestId.New(), "A");

		entity.Equals(null).Should().BeFalse();
	}

	/// <summary>
	/// Verifies that same reference returns true.
	/// </summary>
	[Fact]
	public void Equals_SameReference_ShouldBeTrue()
	{
		var entity = new TestEntity(TestId.New(), "A");

		entity.Equals(entity).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that comparing to different type returns false.
	/// </summary>
	[Fact]
	public void Equals_DifferentType_ShouldBeFalse()
	{
		var entity = new TestEntity(TestId.New(), "A");

		entity.Equals("not an entity").Should().BeFalse();
	}

	/// <summary>
	/// Verifies that entities with same ID have same hash code.
	/// </summary>
	[Fact]
	public void GetHashCode_SameId_ShouldBeEqual()
	{
		var id = TestId.New();
		var entity1 = new TestEntity(id, "A");
		var entity2 = new TestEntity(id, "B");

		entity1.GetHashCode().Should().Be(entity2.GetHashCode());
	}

	/// <summary>
	/// Verifies the == operator.
	/// </summary>
	[Fact]
	public void EqualityOperator_SameId_ShouldReturnTrue()
	{
		var id = TestId.New();
		var entity1 = new TestEntity(id, "A");
		var entity2 = new TestEntity(id, "B");

		(entity1 == entity2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies the != operator.
	/// </summary>
	[Fact]
	public void InequalityOperator_DifferentId_ShouldReturnTrue()
	{
		var entity1 = new TestEntity(TestId.New(), "A");
		var entity2 = new TestEntity(TestId.New(), "B");

		(entity1 != entity2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that == with both null returns true.
	/// </summary>
	[Fact]
	public void EqualityOperator_BothNull_ShouldReturnTrue()
	{
		TestEntity? a = null;
		TestEntity? b = null;

		(a == b).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that == with one null returns false.
	/// </summary>
	[Fact]
	public void EqualityOperator_OneNull_ShouldReturnFalse()
	{
		var entity = new TestEntity(TestId.New(), "A");

		(entity == null).Should().BeFalse();
		(null == entity).Should().BeFalse();
	}
	#endregion

	#region Audit Tests
	/// <summary>
	/// Verifies that InitializeAudit creates audit metadata.
	/// </summary>
	[Fact]
	public void InitializeAudit_ShouldCreateAuditInfo()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		entity.PerformInitializeAudit(now, "admin");

		entity.Audit.Should().NotBeNull();
		entity.Audit.CreatedBy.Should().Be("admin");
		entity.Audit.CreatedOnUtc.Should().Be(now);
	}

	/// <summary>
	/// Verifies that Touch updates modification metadata.
	/// </summary>
	[Fact]
	public void Touch_AfterInit_ShouldUpdateModification()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var created = DateTime.UtcNow;
		entity.PerformInitializeAudit(created, "admin");

		var modified = created.AddMinutes(5);
		entity.PerformTouch(modified, "editor");

		entity.Audit.ModifiedBy.Should().Be("editor");
		entity.Audit.ModifiedOnUtc.Should().Be(modified);
	}

	/// <summary>
	/// Verifies that Touch without InitializeAudit throws.
	/// </summary>
	[Fact]
	public void Touch_WithoutInit_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformTouch(DateTime.UtcNow, "admin");

		act.Should().Throw<InvalidOperationException>();
	}

	/// <summary>
	/// Verifies that SetAudit assigns audit metadata.
	/// </summary>
	[Fact]
	public void SetAudit_ShouldAssignAuditInfo()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		entity.PerformSetAudit(audit);

		entity.Audit.Should().Be(audit);
	}

	/// <summary>
	/// Verifies that SetAudit with null throws.
	/// </summary>
	[Fact]
	public void SetAudit_WithNull_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformSetAudit(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that IAuditableEntity.SetAudit works through the interface.
	/// </summary>
	[Fact]
	public void IAuditableEntity_SetAudit_ShouldWork()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		((IAuditableEntity)entity).SetAudit(audit);

		entity.Audit.Should().Be(audit);
	}
	#endregion

	#region Soft-Delete Tests
	/// <summary>
	/// Verifies that SoftDelete marks entity as deleted.
	/// </summary>
	[Fact]
	public void SoftDelete_ShouldMarkAsDeleted()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		entity.PerformSoftDelete(now, "admin");

		entity.IsDeleted.Should().BeTrue();
		entity.DeletedBy.Should().Be("admin");
		entity.DeletedOnUtc.Should().Be(now);
	}

	/// <summary>
	/// Verifies that SoftDelete is idempotent.
	/// </summary>
	[Fact]
	public void SoftDelete_WhenAlreadyDeleted_ShouldBeIdempotent()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		entity.PerformSoftDelete(now, "admin");
		entity.PerformSoftDelete(now.AddMinutes(5), "other");

		entity.DeletedBy.Should().Be("admin");
		entity.DeletedOnUtc.Should().Be(now);
	}

	/// <summary>
	/// Verifies that SoftDelete with non-UTC date throws.
	/// </summary>
	[Fact]
	public void SoftDelete_WithNonUtcDate_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformSoftDelete(DateTime.Now, "admin");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that SoftDelete with empty actor throws.
	/// </summary>
	[Fact]
	public void SoftDelete_WithEmptyActor_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformSoftDelete(DateTime.UtcNow, "");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that SoftDelete with DateTime.MinValue throws.
	/// </summary>
	[Fact]
	public void SoftDelete_WithMinDate_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformSoftDelete(DateTime.MinValue, "admin");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that SoftDelete with DateTime.MaxValue throws.
	/// </summary>
	[Fact]
	public void SoftDelete_WithMaxDate_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		var act = () => entity.PerformSoftDelete(DateTime.MaxValue, "admin");

		act.Should().Throw<ArgumentException>();
	}
	#endregion

	#region Restore Tests
	/// <summary>
	/// Verifies that Restore undoes a soft-delete.
	/// </summary>
	[Fact]
	public void Restore_AfterSoftDelete_ShouldClearDeletionState()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		entity.PerformSoftDelete(now, "admin");
		entity.PerformRestore(now.AddMinutes(5), "admin");

		entity.IsDeleted.Should().BeFalse();
		entity.DeletedBy.Should().BeNull();
		entity.DeletedOnUtc.Should().BeNull();
	}

	/// <summary>
	/// Verifies that Restore is idempotent when not deleted.
	/// </summary>
	[Fact]
	public void Restore_WhenNotDeleted_ShouldBeIdempotent()
	{
		var entity = new TestEntity(TestId.New(), "Test");

		entity.PerformRestore(DateTime.UtcNow, "admin");

		entity.IsDeleted.Should().BeFalse();
	}

	/// <summary>
	/// Verifies that ISoftDeletableEntity.MarkDeleted works through the interface.
	/// </summary>
	[Fact]
	public void ISoftDeletableEntity_MarkDeleted_ShouldWork()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		((ISoftDeletableEntity)entity).MarkDeleted(now, "admin");

		entity.IsDeleted.Should().BeTrue();
	}

	/// <summary>
	/// Verifies that ISoftDeletableEntity.MarkRestored works through the interface.
	/// </summary>
	[Fact]
	public void ISoftDeletableEntity_MarkRestored_ShouldWork()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var now = DateTime.UtcNow;

		((ISoftDeletableEntity)entity).MarkDeleted(now, "admin");
		((ISoftDeletableEntity)entity).MarkRestored(now.AddMinutes(5), "admin");

		entity.IsDeleted.Should().BeFalse();
	}
	#endregion

	#region Restore Validation Guard Tests
	/// <summary>
	/// Verifies that Restore with non-UTC date throws.
	/// </summary>
	[Fact]
	public void Restore_WithNonUtcDate_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		entity.PerformSoftDelete(DateTime.UtcNow, "admin");

		var act = () => entity.PerformRestore(DateTime.Now, "admin");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Restore with empty actor throws.
	/// </summary>
	[Fact]
	public void Restore_WithEmptyActor_ShouldThrow()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		entity.PerformSoftDelete(DateTime.UtcNow, "admin");

		var act = () => entity.PerformRestore(DateTime.UtcNow, "");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Restore updates audit modification when audit is initialized.
	/// </summary>
	[Fact]
	public void Restore_WithAudit_ShouldUpdateModification()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		entity.PerformInitializeAudit(DateTime.UtcNow, "creator");
		entity.PerformSoftDelete(DateTime.UtcNow.AddMinutes(1), "admin");

		var restoreTime = DateTime.UtcNow.AddMinutes(2);
		entity.PerformRestore(restoreTime, "restorer");

		entity.Audit.ModifiedBy.Should().Be("restorer");
	}
	#endregion

	#region SoftDelete Audit Integration Tests
	/// <summary>
	/// Verifies that SoftDelete updates audit when audit is initialized.
	/// </summary>
	[Fact]
	public void SoftDelete_WithAudit_ShouldUpdateModification()
	{
		var entity = new TestEntity(TestId.New(), "Test");
		var created = DateTime.UtcNow;
		entity.PerformInitializeAudit(created, "admin");

		var deleted = created.AddMinutes(10);
		entity.PerformSoftDelete(deleted, "deleter");

		entity.Audit.ModifiedBy.Should().Be("deleter");
		entity.Audit.ModifiedOnUtc.Should().Be(deleted);
	}
	#endregion
}
