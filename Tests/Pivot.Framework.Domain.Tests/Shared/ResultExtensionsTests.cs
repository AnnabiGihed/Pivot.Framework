using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ResultExtensions"/>.
///              Verifies Ensure, Map, and SafeMap extension methods.
/// </summary>
public class ResultExtensionsTests
{
	#region Ensure Tests
	/// <summary>
	/// Verifies that Ensure returns the same result when predicate passes.
	/// </summary>
	[Fact]
	public void Ensure_PredicatePasses_ShouldReturnSameResult()
	{
		var result = Result.Success(10);
		var error = new Error("ERR", "Too small");

		var ensured = result.Ensure(v => v > 5, error);

		ensured.IsSuccess.Should().BeTrue();
		ensured.Value.Should().Be(10);
	}

	/// <summary>
	/// Verifies that Ensure returns failure when predicate fails.
	/// </summary>
	[Fact]
	public void Ensure_PredicateFails_ShouldReturnFailure()
	{
		var result = Result.Success(3);
		var error = new Error("ERR", "Too small");

		var ensured = result.Ensure(v => v > 5, error);

		ensured.IsFailure.Should().BeTrue();
		ensured.Error.Should().Be(error);
	}

	/// <summary>
	/// Verifies that Ensure on a failure result returns the original failure unchanged.
	/// </summary>
	[Fact]
	public void Ensure_OnFailure_ShouldReturnOriginalFailure()
	{
		var originalError = new Error("ORIG", "Original");
		var result = Result.Failure<int>(originalError);
		var newError = new Error("NEW", "New");

		var ensured = result.Ensure(v => v > 5, newError);

		ensured.IsFailure.Should().BeTrue();
		ensured.Error.Should().Be(originalError);
	}

	/// <summary>
	/// Verifies that Ensure with custom exception type preserves it.
	/// </summary>
	[Fact]
	public void Ensure_WithCustomExceptionType_ShouldPreserve()
	{
		var result = Result.Success(3);
		var error = new Error("ERR", "msg");

		var ensured = result.Ensure(v => v > 5, error, ResultExceptionType.NotFound);

		ensured.ResultExceptionType.Should().Be(ResultExceptionType.NotFound);
	}
	#endregion

	#region Map Tests
	/// <summary>
	/// Verifies that Map transforms a success result's value.
	/// </summary>
	[Fact]
	public void Map_OnSuccess_ShouldTransformValue()
	{
		var result = Result.Success(5);

		var mapped = result.Map(v => v * 2);

		mapped.IsSuccess.Should().BeTrue();
		mapped.Value.Should().Be(10);
	}

	/// <summary>
	/// Verifies that Map propagates failure without calling the mapping function.
	/// </summary>
	[Fact]
	public void Map_OnFailure_ShouldPropagateFailure()
	{
		var error = new Error("ERR", "Failed");
		var result = Result.Failure<int>(error, ResultExceptionType.NotFound);

		var mapped = result.Map(v => v.ToString());

		mapped.IsFailure.Should().BeTrue();
		mapped.Error.Should().Be(error);
		mapped.ResultExceptionType.Should().Be(ResultExceptionType.NotFound);
	}

	/// <summary>
	/// Verifies that Map to a different type works.
	/// </summary>
	[Fact]
	public void Map_ToDifferentType_ShouldWork()
	{
		var result = Result.Success(42);

		var mapped = result.Map(v => $"Value: {v}");

		mapped.IsSuccess.Should().BeTrue();
		mapped.Value.Should().Be("Value: 42");
	}
	#endregion

	#region SafeMap Tests
	/// <summary>
	/// Verifies that SafeMap transforms value on success.
	/// </summary>
	[Fact]
	public void SafeMap_OnSuccess_ShouldTransformValue()
	{
		var result = Result.Success(10);

		var mapped = result.SafeMap(v => v * 3);

		mapped.IsSuccess.Should().BeTrue();
		mapped.Value.Should().Be(30);
	}

	/// <summary>
	/// Verifies that SafeMap catches exceptions and returns failure.
	/// </summary>
	[Fact]
	public void SafeMap_WhenMappingThrows_ShouldReturnFailure()
	{
		var result = Result.Success(10);

		var mapped = result.SafeMap<int, int>(v => throw new InvalidOperationException("boom"));

		mapped.IsFailure.Should().BeTrue();
		mapped.Error.Code.Should().Be("Error.SystemError");
	}

	/// <summary>
	/// Verifies that SafeMap uses custom error factory when provided.
	/// </summary>
	[Fact]
	public void SafeMap_WithCustomErrorFactory_ShouldUseIt()
	{
		var result = Result.Success(10);

		var mapped = result.SafeMap<int, int>(
			v => throw new InvalidOperationException("boom"),
			ex => new Error("CUSTOM", ex.Message));

		mapped.IsFailure.Should().BeTrue();
		mapped.Error.Code.Should().Be("CUSTOM");
		mapped.Error.Message.Should().Be("boom");
	}

	/// <summary>
	/// Verifies that SafeMap on failure propagates the original failure.
	/// </summary>
	[Fact]
	public void SafeMap_OnFailure_ShouldPropagateFailure()
	{
		var error = new Error("ERR", "Original");
		var result = Result.Failure<int>(error);

		var mapped = result.SafeMap(v => v * 2);

		mapped.IsFailure.Should().BeTrue();
		mapped.Error.Should().Be(error);
	}
	#endregion
}
