using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="Error"/>.
///              Verifies construction, equality semantics, operators, factory methods,
///              implicit conversion, and predefined error instances.
/// </summary>
public class ErrorTests
{
	#region Constructor Tests
	/// <summary>
	/// Verifies that the constructor assigns code and message correctly.
	/// </summary>
	[Fact]
	public void Constructor_ShouldAssignCodeAndMessage()
	{
		var error = new Error("ERR001", "Something went wrong");

		error.Code.Should().Be("ERR001");
		error.Message.Should().Be("Something went wrong");
	}

	/// <summary>
	/// Verifies that the constructor throws when code is null.
	/// </summary>
	[Fact]
	public void Constructor_WithNullCode_ShouldThrow()
	{
		var act = () => new Error(null!, "message");

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that the constructor throws when message is null.
	/// </summary>
	[Fact]
	public void Constructor_WithNullMessage_ShouldThrow()
	{
		var act = () => new Error("code", null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Predefined Instances Tests
	/// <summary>
	/// Verifies the <see cref="Error.None"/> predefined instance.
	/// </summary>
	[Fact]
	public void None_ShouldHaveEmptyCodeAndMessage()
	{
		Error.None.Code.Should().BeEmpty();
		Error.None.Message.Should().BeEmpty();
	}

	/// <summary>
	/// Verifies the <see cref="Error.NullValue"/> predefined instance.
	/// </summary>
	[Fact]
	public void NullValue_ShouldHaveExpectedCode()
	{
		Error.NullValue.Code.Should().Be("Error.NullValue");
		Error.NullValue.Message.Should().NotBeNullOrWhiteSpace();
	}
	#endregion

	#region Equality Tests
	/// <summary>
	/// Verifies that two errors with same code and message are equal.
	/// </summary>
	[Fact]
	public void Equals_SameCodeAndMessage_ShouldBeTrue()
	{
		var error1 = new Error("ERR001", "msg");
		var error2 = new Error("ERR001", "msg");

		error1.Equals(error2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that errors with different codes are not equal.
	/// </summary>
	[Fact]
	public void Equals_DifferentCode_ShouldBeFalse()
	{
		var error1 = new Error("ERR001", "msg");
		var error2 = new Error("ERR002", "msg");

		error1.Equals(error2).Should().BeFalse();
	}

	/// <summary>
	/// Verifies that errors with different messages are not equal.
	/// </summary>
	[Fact]
	public void Equals_DifferentMessage_ShouldBeFalse()
	{
		var error1 = new Error("ERR001", "msg1");
		var error2 = new Error("ERR001", "msg2");

		error1.Equals(error2).Should().BeFalse();
	}

	/// <summary>
	/// Verifies equality with null returns false.
	/// </summary>
	[Fact]
	public void Equals_WithNull_ShouldBeFalse()
	{
		var error = new Error("ERR001", "msg");

		error.Equals(null).Should().BeFalse();
	}

	/// <summary>
	/// Verifies object-level equality.
	/// </summary>
	[Fact]
	public void Equals_ObjectOverload_ShouldWork()
	{
		var error1 = new Error("ERR001", "msg");
		object error2 = new Error("ERR001", "msg");

		error1.Equals(error2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies that equal errors have the same hash code.
	/// </summary>
	[Fact]
	public void GetHashCode_SameErrors_ShouldBeEqual()
	{
		var error1 = new Error("ERR001", "msg");
		var error2 = new Error("ERR001", "msg");

		error1.GetHashCode().Should().Be(error2.GetHashCode());
	}
	#endregion

	#region Operator Tests
	/// <summary>
	/// Verifies the == operator for equal errors.
	/// </summary>
	[Fact]
	public void EqualityOperator_SameErrors_ShouldReturnTrue()
	{
		var error1 = new Error("ERR001", "msg");
		var error2 = new Error("ERR001", "msg");

		(error1 == error2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies the != operator for different errors.
	/// </summary>
	[Fact]
	public void InequalityOperator_DifferentErrors_ShouldReturnTrue()
	{
		var error1 = new Error("ERR001", "msg");
		var error2 = new Error("ERR002", "msg");

		(error1 != error2).Should().BeTrue();
	}

	/// <summary>
	/// Verifies the == operator with null on left side.
	/// </summary>
	[Fact]
	public void EqualityOperator_NullLeft_ShouldReturnFalse()
	{
		var error = new Error("ERR001", "msg");

		(null! == error).Should().BeFalse();
	}

	/// <summary>
	/// Verifies the == operator with null on right side.
	/// </summary>
	[Fact]
	public void EqualityOperator_NullRight_ShouldReturnFalse()
	{
		var error = new Error("ERR001", "msg");

		(error == null!).Should().BeFalse();
	}

	/// <summary>
	/// Verifies the == operator with both null.
	/// </summary>
	[Fact]
	public void EqualityOperator_BothNull_ShouldReturnTrue()
	{
		Error? a = null;
		Error? b = null;

		(a == b).Should().BeTrue();
	}

	/// <summary>
	/// Verifies the == operator with same reference.
	/// </summary>
	[Fact]
	public void EqualityOperator_SameReference_ShouldReturnTrue()
	{
		var error = new Error("ERR001", "msg");
		var same = error;

		(error == same).Should().BeTrue();
	}
	#endregion

	#region ToString Tests
	/// <summary>
	/// Verifies that ToString returns the error code.
	/// </summary>
	[Fact]
	public void ToString_ShouldReturnCode()
	{
		var error = new Error("ERR001", "msg");

		error.ToString().Should().Be("ERR001");
	}
	#endregion

	#region Implicit Conversion Tests
	/// <summary>
	/// Verifies implicit conversion to string returns the code.
	/// </summary>
	[Fact]
	public void ImplicitConversion_ToString_ShouldReturnCode()
	{
		var error = new Error("ERR001", "msg");

		string result = error;

		result.Should().Be("ERR001");
	}
	#endregion

	#region Factory Method Tests
	/// <summary>
	/// Verifies <see cref="Error.SystemError"/> factory method.
	/// </summary>
	[Fact]
	public void SystemError_ShouldCreateWithCorrectCode()
	{
		var error = Error.SystemError("Something broke");

		error.Code.Should().Be("Error.SystemError");
		error.Message.Should().Be("Something broke");
	}

	/// <summary>
	/// Verifies <see cref="Error.InvalidValue"/> factory method.
	/// </summary>
	[Fact]
	public void InvalidValue_ShouldCreateWithCorrectCode()
	{
		var error = Error.InvalidValue("Bad data");

		error.Code.Should().Be("Error.InvalidValue");
		error.Message.Should().Be("Bad data");
	}
	#endregion
}
