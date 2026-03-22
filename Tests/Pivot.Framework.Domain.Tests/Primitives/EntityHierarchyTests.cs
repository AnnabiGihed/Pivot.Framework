using FluentAssertions;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Tests for the Entity hierarchy at each level:
///              Entity, AuditableEntity, and FullEntity in isolation.
/// </summary>
public class EntityHierarchyTests
{
	#region Entity<TId> — Identity Only

	/// <summary>
	/// Verifies that a bare entity exposes only an identity property.
	/// </summary>
	[Fact]
	public void BareEntity_ShouldHaveIdOnly()
	{
		var id = TestId.New();
		var entity = new TestBareEntity(id);
		entity.Id.Should().Be(id);
	}

	/// <summary>
	/// Verifies that a bare entity does not implement <see cref="IAuditableEntity"/>.
	/// </summary>
	[Fact]
	public void BareEntity_ShouldNotImplementIAuditableEntity()
	{
		var entity = new TestBareEntity(TestId.New());
		entity.Should().NotBeAssignableTo<IAuditableEntity>();
	}

	/// <summary>
	/// Verifies that a bare entity does not implement <see cref="ISoftDeletableEntity"/>.
	/// </summary>
	[Fact]
	public void BareEntity_ShouldNotImplementISoftDeletableEntity()
	{
		var entity = new TestBareEntity(TestId.New());
		entity.Should().NotBeAssignableTo<ISoftDeletableEntity>();
	}

	/// <summary>
	/// Verifies that two bare entities with the same ID are equal.
	/// </summary>
	[Fact]
	public void BareEntity_Equality_SameId_ShouldBeEqual()
	{
		var id = TestId.New();
		var e1 = new TestBareEntity(id);
		var e2 = new TestBareEntity(id);
		e1.Should().Be(e2);
	}

	/// <summary>
	/// Verifies that two bare entities with different IDs are not equal.
	/// </summary>
	[Fact]
	public void BareEntity_Equality_DifferentId_ShouldNotBeEqual()
	{
		var e1 = new TestBareEntity(TestId.New());
		var e2 = new TestBareEntity(TestId.New());
		e1.Should().NotBe(e2);
	}

	#endregion

	#region AuditableEntity<TId> — Identity + Audit

	/// <summary>
	/// Verifies that an auditable entity implements <see cref="IAuditableEntity"/>.
	/// </summary>
	[Fact]
	public void AuditableEntity_ShouldImplementIAuditableEntity()
	{
		var entity = new TestAuditableEntity(TestId.New());
		entity.Should().BeAssignableTo<IAuditableEntity>();
	}

	/// <summary>
	/// Verifies that an auditable entity does not implement <see cref="ISoftDeletableEntity"/>.
	/// </summary>
	[Fact]
	public void AuditableEntity_ShouldNotImplementISoftDeletableEntity()
	{
		var entity = new TestAuditableEntity(TestId.New());
		entity.Should().NotBeAssignableTo<ISoftDeletableEntity>();
	}

	/// <summary>
	/// Verifies that initializing audit on an auditable entity sets the audit info.
	/// </summary>
	[Fact]
	public void AuditableEntity_InitializeAudit_ShouldSetAuditInfo()
	{
		var entity = new TestAuditableEntity(TestId.New());
		var now = DateTime.UtcNow;
		entity.InitializeAudit(now, "admin");
		entity.Audit.Should().NotBeNull();
		entity.Audit!.CreatedBy.Should().Be("admin");
	}

	/// <summary>
	/// Verifies that Touch updates the modification metadata on an auditable entity.
	/// </summary>
	[Fact]
	public void AuditableEntity_Touch_ShouldUpdateModification()
	{
		var entity = new TestAuditableEntity(TestId.New());
		entity.InitializeAudit(DateTime.UtcNow, "admin");
		var later = DateTime.UtcNow.AddMinutes(5);
		entity.Touch(later, "editor");
		entity.Audit!.ModifiedBy.Should().Be("editor");
	}

	#endregion
}
