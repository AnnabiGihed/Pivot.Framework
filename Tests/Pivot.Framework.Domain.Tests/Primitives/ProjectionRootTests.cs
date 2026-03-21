using FluentAssertions;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="Pivot.Framework.Domain.Primitives.ProjectionRoot{TId}"/>.
///              Verifies construction and identity assignment.
/// </summary>
public class ProjectionRootTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that constructing a projection root with a valid ID assigns it correctly.
	/// </summary>
	[Fact]
	public void Constructor_WithValidId_ShouldAssignId()
	{
		var id = TestId.New();

		var projection = new TestProjectionRoot(id, "Test Projection");

		projection.Id.Should().Be(id);
		projection.DisplayName.Should().Be("Test Projection");
	}

	/// <summary>
	/// Verifies that constructing a projection root with null ID throws.
	/// </summary>
	[Fact]
	public void Constructor_WithNullId_ShouldThrow()
	{
		var act = () => new TestProjectionRoot(null!, "Test");

		act.Should().Throw<ArgumentNullException>();
	}
	/// <summary>
	/// Verifies that the parameterless constructor sets Id to default.
	/// </summary>
	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultId()
	{
		var projection = new TestProjectionRoot();

		projection.Id.Should().BeNull();
	}
	#endregion
}
