using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for <see cref="AuditableEntity{TId}"/> in isolation.
///              Derives from AuditableEntity without soft-delete capabilities.
///              Exposes protected methods for unit test assertions.
/// </summary>
internal sealed class TestAuditableEntity : AuditableEntity<TestId>
{
	public string Name { get; set; } = string.Empty;
	public TestAuditableEntity(TestId id) : base(id) { }
	public TestAuditableEntity() { }

	/// <summary>
	/// Exposes <see cref="AuditableEntity{TId}.InitializeAudit"/> for testing.
	/// </summary>
	public new void InitializeAudit(DateTime createdOnUtc, string createdBy)
	{
		base.InitializeAudit(createdOnUtc, createdBy);
	}

	/// <summary>
	/// Exposes <see cref="AuditableEntity{TId}.Touch"/> for testing.
	/// </summary>
	public new void Touch(DateTime modifiedOnUtc, string modifiedBy)
	{
		base.Touch(modifiedOnUtc, modifiedBy);
	}
}
