using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.Tests.TestDoubles;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Test double for <see cref="ProjectionRoot{TId}"/>.
///              A minimal read model for unit test assertions.
/// </summary>
public sealed class TestProjectionRoot : ProjectionRoot<TestId>
{
	#region Properties
	/// <summary>
	/// Gets the display name.
	/// </summary>
	public string DisplayName { get; init; } = string.Empty;
	#endregion

	#region Constructors
	/// <summary>
	/// Parameterless constructor for EF Core materialisation testing.
	/// </summary>
	public TestProjectionRoot() : base() { }

	/// <summary>
	/// Initialises a new <see cref="TestProjectionRoot"/> with identity and display name.
	/// </summary>
	public TestProjectionRoot(TestId id, string displayName) : base(id)
	{
		DisplayName = displayName;
	}
	#endregion
}
