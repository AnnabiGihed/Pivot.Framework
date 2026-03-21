using FluentAssertions;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="AuditInfo"/>.
///              Verifies factory creation, modification, update, equality, and guard clauses.
/// </summary>
public class AuditInfoTests
{
	#region Create Tests
	/// <summary>
	/// Verifies that <see cref="AuditInfo.Create"/> initialises all fields correctly.
	/// </summary>
	[Fact]
	public void Create_ShouldInitialiseAllFields()
	{
		var now = DateTime.UtcNow;

		var audit = AuditInfo.Create(now, "admin");

		audit.CreatedBy.Should().Be("admin");
		audit.ModifiedBy.Should().Be("admin");
		audit.CreatedOnUtc.Should().Be(now);
		audit.ModifiedOnUtc.Should().Be(now);
	}

	/// <summary>
	/// Verifies that Create with null author throws.
	/// </summary>
	[Fact]
	public void Create_WithNullAuthor_ShouldThrow()
	{
		var act = () => AuditInfo.Create(DateTime.UtcNow, null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that Create with whitespace author throws.
	/// </summary>
	[Fact]
	public void Create_WithWhitespaceAuthor_ShouldThrow()
	{
		var act = () => AuditInfo.Create(DateTime.UtcNow, "   ");

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that Create with non-UTC date throws.
	/// </summary>
	[Fact]
	public void Create_WithNonUtcDate_ShouldThrow()
	{
		var act = () => AuditInfo.Create(DateTime.Now, "admin");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Create with DateTime.MinValue throws.
	/// </summary>
	[Fact]
	public void Create_WithMinDate_ShouldThrow()
	{
		var act = () => AuditInfo.Create(DateTime.MinValue, "admin");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Create with DateTime.MaxValue throws.
	/// </summary>
	[Fact]
	public void Create_WithMaxDate_ShouldThrow()
	{
		var act = () => AuditInfo.Create(DateTime.MaxValue, "admin");

		act.Should().Throw<ArgumentException>();
	}
	#endregion

	#region Modify Tests
	/// <summary>
	/// Verifies that <see cref="AuditInfo.Modify"/> updates modification fields.
	/// </summary>
	[Fact]
	public void Modify_ShouldUpdateModificationFields()
	{
		var created = DateTime.UtcNow;
		var audit = AuditInfo.Create(created, "admin");

		var modified = created.AddHours(1);
		audit.Modify(modified, "editor");

		audit.ModifiedBy.Should().Be("editor");
		audit.ModifiedOnUtc.Should().Be(modified);
		audit.CreatedBy.Should().Be("admin");
		audit.CreatedOnUtc.Should().Be(created);
	}

	/// <summary>
	/// Verifies that Modify with null author throws.
	/// </summary>
	[Fact]
	public void Modify_WithNullAuthor_ShouldThrow()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var act = () => audit.Modify(DateTime.UtcNow, null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that Modify with non-UTC date throws.
	/// </summary>
	[Fact]
	public void Modify_WithNonUtcDate_ShouldThrow()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var act = () => audit.Modify(DateTime.Now, "editor");

		act.Should().Throw<ArgumentException>();
	}
	#endregion

	#region Update Tests
	/// <summary>
	/// Verifies that <see cref="AuditInfo.Update"/> replaces all fields.
	/// </summary>
	[Fact]
	public void Update_ShouldReplaceAllFields()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var newCreated = DateTime.UtcNow.AddDays(-1);
		var newModified = DateTime.UtcNow;
		audit.Update("newCreator", "newModifier", newCreated, newModified);

		audit.CreatedBy.Should().Be("newCreator");
		audit.ModifiedBy.Should().Be("newModifier");
		audit.CreatedOnUtc.Should().Be(newCreated);
		audit.ModifiedOnUtc.Should().Be(newModified);
	}

	/// <summary>
	/// Verifies that Update with invalid dates throws.
	/// </summary>
	[Fact]
	public void Update_WithNonUtcCreatedDate_ShouldThrow()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var act = () => audit.Update("a", "b", DateTime.Now, DateTime.UtcNow);

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Update with invalid modified date throws.
	/// </summary>
	[Fact]
	public void Update_WithNonUtcModifiedDate_ShouldThrow()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var act = () => audit.Update("a", "b", DateTime.UtcNow, DateTime.Now);

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Update with null createdBy throws.
	/// </summary>
	[Fact]
	public void Update_WithNullCreatedBy_ShouldThrow()
	{
		var audit = AuditInfo.Create(DateTime.UtcNow, "admin");

		var act = () => audit.Update(null!, "b", DateTime.UtcNow, DateTime.UtcNow);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Equality Tests
	/// <summary>
	/// Verifies that two AuditInfo with same values are equal.
	/// </summary>
	[Fact]
	public void Equals_SameValues_ShouldBeTrue()
	{
		var now = DateTime.UtcNow;
		var audit1 = AuditInfo.Create(now, "admin");
		var audit2 = AuditInfo.Create(now, "admin");

		audit1.Should().Be(audit2);
	}

	/// <summary>
	/// Verifies that two AuditInfo with different values are not equal.
	/// </summary>
	[Fact]
	public void Equals_DifferentCreatedBy_ShouldBeFalse()
	{
		var now = DateTime.UtcNow;
		var audit1 = AuditInfo.Create(now, "admin");
		var audit2 = AuditInfo.Create(now, "other");

		audit1.Should().NotBe(audit2);
	}

	/// <summary>
	/// Verifies that equal AuditInfo instances have the same hash code.
	/// </summary>
	[Fact]
	public void GetHashCode_SameValues_ShouldBeEqual()
	{
		var now = DateTime.UtcNow;
		var audit1 = AuditInfo.Create(now, "admin");
		var audit2 = AuditInfo.Create(now, "admin");

		audit1.GetHashCode().Should().Be(audit2.GetHashCode());
	}
	#endregion
}
