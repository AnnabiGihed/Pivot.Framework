using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for <see cref="FullEntity{TId}"/>.
///              Exposes protected methods for unit test assertions.
/// </summary>
public sealed class TestEntity : FullEntity<TestId>
{
	#region Properties
	/// <summary>
	/// Gets the name of this test entity.
	/// </summary>
	public string Name { get; private set; } = string.Empty;
	#endregion

	#region Constructors
	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	private TestEntity() : base() { }

	/// <summary>
	/// Initialises a new <see cref="TestEntity"/> with the specified identity and name.
	/// </summary>
	public TestEntity(TestId id, string name) : base(id)
	{
		Name = name;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Exposes <see cref="AuditableEntity{TId}.InitializeAudit"/> for testing.
	/// </summary>
	public void PerformInitializeAudit(DateTime createdOnUtc, string createdBy)
	{
		InitializeAudit(createdOnUtc, createdBy);
	}

	/// <summary>
	/// Exposes <see cref="AuditableEntity{TId}.Touch"/> for testing.
	/// </summary>
	public void PerformTouch(DateTime modifiedOnUtc, string modifiedBy)
	{
		Touch(modifiedOnUtc, modifiedBy);
	}

	/// <summary>
	/// Exposes <see cref="AuditableEntity{TId}.SetAudit"/> for testing.
	/// </summary>
	public void PerformSetAudit(AuditInfo audit)
	{
		SetAudit(audit);
	}

	/// <summary>
	/// Exposes <see cref="FullEntity{TId}.SoftDelete"/> for testing.
	/// </summary>
	public void PerformSoftDelete(DateTime deletedOnUtc, string deletedBy)
	{
		SoftDelete(deletedOnUtc, deletedBy);
	}

	/// <summary>
	/// Exposes <see cref="FullEntity{TId}.Restore"/> for testing.
	/// </summary>
	public void PerformRestore(DateTime restoredOnUtc, string restoredBy)
	{
		Restore(restoredOnUtc, restoredBy);
	}
	#endregion
}
