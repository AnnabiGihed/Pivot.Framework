using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for a strongly-typed GUID identifier.
///              Used by unit tests to construct aggregate roots and entities
///              without depending on production identifier types.
/// </summary>
public sealed record TestId : StronglyTypedGuidId<TestId>
{
	/// <summary>
	/// Initialises a new <see cref="TestId"/> with the specified GUID value.
	/// </summary>
	/// <param name="value">The underlying GUID value.</param>
	public TestId(Guid value) : base(value) { }

	/// <summary>
	/// Creates a new <see cref="TestId"/> with a randomly generated GUID.
	/// </summary>
	/// <returns>A new unique <see cref="TestId"/>.</returns>
	public static TestId New() => new(Guid.NewGuid());
}
