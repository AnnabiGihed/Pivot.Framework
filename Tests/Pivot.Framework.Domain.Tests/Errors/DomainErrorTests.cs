using FluentAssertions;
using Pivot.Framework.Domain.Errors;

namespace Pivot.Framework.Domain.Tests.Errors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="DomainError"/>.
///              Verifies construction, serialisation, deserialisation, label helpers,
///              equality semantics, and validation guards.
/// </summary>
public class DomainErrorTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that the constructor assigns code and message correctly.
	/// </summary>
	[Fact]
	public void Constructor_ShouldAssignCodeAndMessage()
	{
		var error = new DomainError("Order.Status.Invalid", "Status is invalid");

		error.Code.Should().Be("Order.Status.Invalid");
		error.Message.Should().Be("Status is invalid");
	}

	/// <summary>
	/// Verifies that null or whitespace code throws.
	/// </summary>
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithInvalidCode_ShouldThrow(string? code)
	{
		var act = () => new DomainError(code!, "message");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that null message throws.
	/// </summary>
	[Fact]
	public void Constructor_WithNullMessage_ShouldThrow()
	{
		var act = () => new DomainError("code", null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Serialisation Tests
	/// <summary>
	/// Verifies that <see cref="DomainError.Serialize"/> produces the expected format.
	/// </summary>
	[Fact]
	public void Serialize_ShouldProduceCodeSeparatorMessage()
	{
		var error = new DomainError("ERR001", "Something failed");

		var serialized = error.Serialize();

		serialized.Should().Be("ERR001||Something failed");
	}

	/// <summary>
	/// Verifies that <see cref="DomainError.Deserialize"/> can parse a valid serialized error.
	/// </summary>
	[Fact]
	public void Deserialize_ValidString_ShouldParse()
	{
		var result = DomainError.Deserialize("ERR001||Something failed");

		result.Code.Should().Be("ERR001");
		result.Message.Should().Be("Something failed");
	}

	/// <summary>
	/// Verifies that Deserialize handles messages containing the separator.
	/// </summary>
	[Fact]
	public void Deserialize_MessageWithSeparator_ShouldParseCorrectly()
	{
		var result = DomainError.Deserialize("ERR001||Part1||Part2");

		result.Code.Should().Be("ERR001");
		result.Message.Should().Be("Part1||Part2");
	}

	/// <summary>
	/// Verifies that Deserialize with null or whitespace throws.
	/// </summary>
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Deserialize_WithInvalidInput_ShouldThrow(string? input)
	{
		var act = () => DomainError.Deserialize(input!);

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that Deserialize with no separator throws FormatException.
	/// </summary>
	[Fact]
	public void Deserialize_WithNoSeparator_ShouldThrowFormatException()
	{
		var act = () => DomainError.Deserialize("NoSeparatorHere");

		act.Should().Throw<FormatException>();
	}

	/// <summary>
	/// Verifies serialise/deserialise roundtrip.
	/// </summary>
	[Fact]
	public void Serialize_Deserialize_ShouldRoundtrip()
	{
		var error = new DomainError("Order.NotFound", "Order was not found");
		var serialized = error.Serialize();
		var deserialized = DomainError.Deserialize(serialized);

		deserialized.Code.Should().Be(error.Code);
		deserialized.Message.Should().Be(error.Message);
	}
	#endregion

	#region Label Helper Tests
	/// <summary>
	/// Verifies that <see cref="DomainError.ForField"/> with null returns a single space.
	/// </summary>
	[Fact]
	public void ForField_WithNull_ShouldReturnSpace()
	{
		var result = DomainError.ForField(null);

		result.Should().Be(" ");
	}

	/// <summary>
	/// Verifies that <see cref="DomainError.ForField"/> with name includes the name.
	/// </summary>
	[Fact]
	public void ForField_WithName_ShouldContainName()
	{
		var result = DomainError.ForField("Email");

		result.Should().Contain("Email");
	}

	/// <summary>
	/// Verifies that <see cref="DomainError.ValueForField"/> with both name and value includes both.
	/// </summary>
	[Fact]
	public void ValueForField_WithNameAndValue_ShouldContainBoth()
	{
		var result = DomainError.ValueForField("Email", "test@test.com");

		result.Should().Contain("Email");
		result.Should().Contain("test@test.com");
	}

	/// <summary>
	/// Verifies that ValueForField with null name and null value returns space.
	/// </summary>
	[Fact]
	public void ValueForField_BothNull_ShouldReturnSpace()
	{
		var result = DomainError.ValueForField(null, null);

		result.Should().Be(" ");
	}

	/// <summary>
	/// Verifies that ValueForField with only name includes the name.
	/// </summary>
	[Fact]
	public void ValueForField_OnlyName_ShouldContainName()
	{
		var result = DomainError.ValueForField("Email", null);

		result.Should().Contain("Email");
	}

	/// <summary>
	/// Verifies that ValueForField with only value includes the value.
	/// </summary>
	[Fact]
	public void ValueForField_OnlyValue_ShouldContainValue()
	{
		var result = DomainError.ValueForField(null, "test@test.com");

		result.Should().Contain("test@test.com");
	}
	#endregion

	#region Equality Tests
	/// <summary>
	/// Verifies that two DomainErrors with same code are equal (regardless of message).
	/// </summary>
	[Fact]
	public void Equals_SameCode_ShouldBeTrue()
	{
		var error1 = new DomainError("ERR001", "Message A");
		var error2 = new DomainError("ERR001", "Message B");

		error1.Should().Be(error2);
	}

	/// <summary>
	/// Verifies that two DomainErrors with different codes are not equal.
	/// </summary>
	[Fact]
	public void Equals_DifferentCode_ShouldBeFalse()
	{
		var error1 = new DomainError("ERR001", "msg");
		var error2 = new DomainError("ERR002", "msg");

		error1.Should().NotBe(error2);
	}

	/// <summary>
	/// Verifies that equal DomainErrors have the same hash code.
	/// </summary>
	[Fact]
	public void GetHashCode_SameCode_ShouldBeEqual()
	{
		var error1 = new DomainError("ERR001", "A");
		var error2 = new DomainError("ERR001", "B");

		error1.GetHashCode().Should().Be(error2.GetHashCode());
	}
	#endregion

	#region ValidateDescriptors Tests (via BaseDomainErrors)
	/// <summary>
	/// Verifies that descriptor validation throws for null code descriptor
	/// (tested indirectly through BaseDomainErrors which calls ValidateDescriptors internally).
	/// </summary>
	[Fact]
	public void BaseDomainErrors_WithNullCodeDescriptor_ShouldThrow()
	{
		var act = () => BaseDomainErrors.General.NotFound(null, null!, "prop");

		act.Should().Throw<ArgumentException>();
	}

	/// <summary>
	/// Verifies that descriptor validation throws for null property descriptor.
	/// </summary>
	[Fact]
	public void BaseDomainErrors_WithNullPropertyDescriptor_ShouldThrow()
	{
		var act = () => BaseDomainErrors.General.NotFound(null, "code", null!);

		act.Should().Throw<ArgumentException>();
	}
	#endregion
}
