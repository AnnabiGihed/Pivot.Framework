using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="Result"/>.
///              Verifies success/failure creation, constructor guards, and factory methods.
/// </summary>
public class ResultTests
{
	#region Success Tests
	/// <summary>
	/// Verifies that <see cref="Result.Success"/> creates a successful result.
	/// </summary>
	[Fact]
	public void Success_ShouldReturnSuccessResult()
	{
		var result = Result.Success();

		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error.Should().Be(Error.None);
		result.ResultExceptionType.Should().Be(ResultExceptionType.None);
	}

	/// <summary>
	/// Verifies that <see cref="Result.Success{TValue}"/> creates a success with value.
	/// </summary>
	[Fact]
	public void SuccessT_ShouldReturnSuccessResultWithValue()
	{
		var result = Result.Success(42);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(42);
		result.ResultExceptionType.Should().Be(ResultExceptionType.None);
	}
	#endregion

	#region Failure Tests
	/// <summary>
	/// Verifies that <see cref="Result.Failure"/> creates a failure result.
	/// </summary>
	[Fact]
	public void Failure_ShouldReturnFailureResult()
	{
		var error = new Error("ERR", "Something failed");

		var result = Result.Failure(error);

		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(error);
		result.ResultExceptionType.Should().Be(ResultExceptionType.BadRequest);
	}

	/// <summary>
	/// Verifies that <see cref="Result.Failure"/> with custom exception type works.
	/// </summary>
	[Fact]
	public void Failure_WithCustomExceptionType_ShouldPreserveType()
	{
		var error = new Error("ERR", "Not found");

		var result = Result.Failure(error, ResultExceptionType.NotFound);

		result.ResultExceptionType.Should().Be(ResultExceptionType.NotFound);
	}

	/// <summary>
	/// Verifies that <see cref="Result.Failure{TValue}"/> creates a typed failure.
	/// </summary>
	[Fact]
	public void FailureT_ShouldReturnTypedFailure()
	{
		var error = new Error("ERR", "Failed");

		var result = Result.Failure<int>(error);

		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(error);
	}

	/// <summary>
	/// Verifies that <see cref="Result.Failure{TValue}"/> with custom exception type works.
	/// </summary>
	[Fact]
	public void FailureT_WithConflict_ShouldPreserveType()
	{
		var error = new Error("ERR", "Conflict");

		var result = Result.Failure<string>(error, ResultExceptionType.Conflict);

		result.ResultExceptionType.Should().Be(ResultExceptionType.Conflict);
	}
	#endregion

	#region Constructor Guard Tests
	/// <summary>
	/// Verifies that creating a success with a non-None error throws.
	/// </summary>
	[Fact]
	public void Constructor_SuccessWithError_ShouldThrow()
	{
		var act = () => Result.Failure(Error.None);

		act.Should().Throw<InvalidOperationException>();
	}
	#endregion

	#region Create Tests
	/// <summary>
	/// Verifies that <see cref="Result.Create{TValue}"/> with non-null value returns success.
	/// </summary>
	[Fact]
	public void Create_WithNonNullValue_ShouldReturnSuccess()
	{
		var result = Result.Create<string>("hello");

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be("hello");
	}

	/// <summary>
	/// Verifies that <see cref="Result.Create{TValue}"/> with null value returns failure.
	/// </summary>
	[Fact]
	public void Create_WithNullValue_ShouldReturnFailure()
	{
		var result = Result.Create<string>(null);

		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be(Error.NullValue);
	}
	#endregion
}
