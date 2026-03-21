using FluentAssertions;
using Pivot.Framework.Domain.Tests.TestDoubles;

namespace Pivot.Framework.Domain.Tests.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="StronglyTypedGuidId{TSelf}"/> via <see cref="TestId"/>.
///              Verifies construction guards, equality semantics, comparison, and string representation.
/// </summary>
public class StronglyTypedGuidIdTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that constructing a <see cref="TestId"/> with a valid <see cref="Guid"/> succeeds
	/// and exposes the correct value.
	/// </summary>
	[Fact]
	public void Constructor_WithValidGuid_ShouldSucceed()
	{
		var guid = Guid.NewGuid();

		var id = new TestId(guid);

		id.Value.Should().Be(guid);
	}

	/// <summary>
	/// Verifies that constructing a <see cref="TestId"/> with <see cref="Guid.Empty"/>
	/// throws <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
	{
		var act = () => new TestId(Guid.Empty);

		act.Should().Throw<ArgumentException>();
	}
	#endregion

	#region ToString Tests
	/// <summary>
	/// Verifies that the underlying <see cref="Guid"/> value is correctly represented as a string.
	/// </summary>
	[Fact]
	public void ToString_ShouldContainGuidValue()
	{
		var guid = Guid.NewGuid();
		var id = new TestId(guid);

		id.Value.ToString().Should().Be(guid.ToString());
	}
	#endregion

	#region Equality Tests
	/// <summary>
	/// Verifies that two <see cref="TestId"/> instances wrapping the same <see cref="Guid"/>
	/// are considered equal.
	/// </summary>
	[Fact]
	public void Equality_SameGuid_ShouldBeEqual()
	{
		var guid = Guid.NewGuid();
		var id1 = new TestId(guid);
		var id2 = new TestId(guid);

		id1.Should().Be(id2);
		(id1 == id2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that two <see cref="TestId"/> instances wrapping different <see cref="Guid"/>
	/// values are not equal.
	/// </summary>
	[Fact]
	public void Equality_DifferentGuid_ShouldNotBeEqual()
	{
		var id1 = TestId.New();
		var id2 = TestId.New();

		id1.Should().NotBe(id2);
	}
	#endregion

	#region CompareTo Tests
	/// <summary>
	/// Verifies that comparing a <see cref="TestId"/> to <c>null</c> returns a positive value,
	/// indicating that a non-null identifier is always greater than null.
	/// </summary>
	[Fact]
	public void CompareTo_Null_ShouldReturnPositive()
	{
		var id = TestId.New();

		id.CompareTo(null).Should().BePositive();
	}

	/// <summary>
	/// Verifies that comparing two identical <see cref="TestId"/> instances returns zero.
	/// </summary>
	[Fact]
	public void CompareTo_SameValue_ShouldReturnZero()
	{
		var guid = Guid.NewGuid();
		var id1 = new TestId(guid);
		var id2 = new TestId(guid);

		id1.CompareTo(id2).Should().Be(0);
	}
	#endregion

	#region ToString Override Tests
	/// <summary>
	/// Verifies that ToString output includes the GUID value.
	/// </summary>
	[Fact]
	public void ToString_Output_ShouldIncludeGuidValue()
	{
		var guid = Guid.NewGuid();
		var id = new TestId(guid);

		id.ToString().Should().Contain(guid.ToString());
	}
	#endregion

	#region HashCode Tests
	/// <summary>
	/// Verifies that two equal IDs produce the same hash code.
	/// </summary>
	[Fact]
	public void GetHashCode_SameValue_ShouldBeEqual()
	{
		var guid = Guid.NewGuid();
		var id1 = new TestId(guid);
		var id2 = new TestId(guid);

		id1.GetHashCode().Should().Be(id2.GetHashCode());
	}
	#endregion
}
