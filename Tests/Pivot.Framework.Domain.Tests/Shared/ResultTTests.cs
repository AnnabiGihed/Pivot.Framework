using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="Result{TValue}"/>.
///              Verifies value access, failure semantics, and implicit conversion.
/// </summary>
public class ResultTTests
{
	#region Value Access Tests
	/// <summary>
	/// Verifies that accessing Value on a success result returns the value.
	/// </summary>
	[Fact]
	public void Value_OnSuccess_ShouldReturnValue()
	{
		var result = Result.Success("hello");

		result.Value.Should().Be("hello");
	}

	/// <summary>
	/// Verifies that accessing Value on a failure result throws.
	/// </summary>
	[Fact]
	public void Value_OnFailure_ShouldThrow()
	{
		var result = Result.Failure<string>(new Error("ERR", "Failed"));

		var act = () => result.Value;

		act.Should().Throw<InvalidOperationException>();
	}
	#endregion

	#region Constructor Guard Tests
	/// <summary>
	/// Verifies that creating a success result with null value throws.
	/// </summary>
	[Fact]
	public void Constructor_SuccessWithNullValue_ShouldThrow()
	{
		var act = () => Result.Success<string>(null!);

		act.Should().Throw<InvalidOperationException>();
	}
	#endregion

	#region Implicit Conversion Tests
	/// <summary>
	/// Verifies that implicit conversion from non-null value creates success.
	/// </summary>
	[Fact]
	public void ImplicitConversion_NonNullValue_ShouldCreateSuccess()
	{
		Result<string> result = "hello";

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be("hello");
	}

	/// <summary>
	/// Verifies that implicit conversion from null value creates failure.
	/// </summary>
	[Fact]
	public void ImplicitConversion_NullValue_ShouldCreateFailure()
	{
		Result<string> result = (string?)null;

		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(Error.NullValue);
	}
	#endregion

	#region ResultExceptionType Tests
	/// <summary>
	/// Verifies that success results always have <see cref="ResultExceptionType.None"/>.
	/// </summary>
	[Fact]
	public void Success_ShouldAlwaysHaveNoneExceptionType()
	{
		var result = Result.Success(42);

		result.ResultExceptionType.Should().Be(ResultExceptionType.None);
	}

	/// <summary>
	/// Verifies that failure results preserve their exception type.
	/// </summary>
	[Fact]
	public void Failure_ShouldPreserveExceptionType()
	{
		var result = Result.Failure<int>(new Error("ERR", "msg"), ResultExceptionType.AccessDenied);

		result.ResultExceptionType.Should().Be(ResultExceptionType.AccessDenied);
	}
	#endregion
}
