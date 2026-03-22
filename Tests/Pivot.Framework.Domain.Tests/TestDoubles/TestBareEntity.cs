using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for <see cref="Entity{TId}"/> in isolation.
///              Derives directly from Entity without audit or soft-delete capabilities.
/// </summary>
internal sealed class TestBareEntity : Entity<TestId>
{
	public string Name { get; set; } = string.Empty;
	public TestBareEntity(TestId id) : base(id) { }
	public TestBareEntity() { }
}
