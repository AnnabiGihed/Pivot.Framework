using FluentAssertions;
using Pivot.Framework.Containers.API.Abstractions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Tests.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ApiResponse{T}"/>.
///              Verifies Ok, Fail, and FromResult factory methods,
///              error normalisation, and default property values.
/// </summary>
public class ApiResponseTests
{
	#region Ok Tests
	/// <summary>
	/// Verifies that <see cref="ApiResponse{T}.Ok"/> creates a success response with data.
	/// </summary>
	[Fact]
	public void Ok_ShouldCreateSuccessResponse()
	{
		var response = ApiResponse<string>.Ok("hello");

		response.Success.Should().BeTrue();
		response.Data.Should().Be("hello");
		response.Errors.Should().BeEmpty();
		response.Message.Should().BeEmpty();
	}

	/// <summary>
	/// Verifies that <see cref="ApiResponse{T}.Ok"/> with message includes it.
	/// </summary>
	[Fact]
	public void Ok_WithMessage_ShouldIncludeMessage()
	{
		var response = ApiResponse<int>.Ok(42, "Success!");

		response.Success.Should().BeTrue();
		response.Data.Should().Be(42);
		response.Message.Should().Be("Success!");
	}
	#endregion

	#region Fail Tests
	/// <summary>
	/// Verifies that <see cref="ApiResponse{T}.Fail"/> creates a failure response.
	/// </summary>
	[Fact]
	public void Fail_ShouldCreateFailureResponse()
	{
		var error = new Error("ERR001", "Something failed");

		var response = ApiResponse<string>.Fail(error);

		response.Success.Should().BeFalse();
		response.Data.Should().BeNull();
		response.Errors.Should().HaveCount(1);
		response.Message.Should().Be("Something failed");
	}

	/// <summary>
	/// Verifies that Fail with empty message falls back to code.
	/// </summary>
	[Fact]
	public void Fail_WithEmptyMessage_ShouldFallbackToCode()
	{
		var error = new Error("ERR001", "");

		var response = ApiResponse<string>.Fail(error);

		response.Message.Should().Be("ERR001");
	}

	/// <summary>
	/// Verifies that Fail with additional errors includes them all.
	/// </summary>
	[Fact]
	public void Fail_WithAdditionalErrors_ShouldIncludeAll()
	{
		var primary = new Error("ERR001", "Primary");
		var extras = new[] { new Error("ERR002", "Second"), new Error("ERR003", "Third") };

		var response = ApiResponse<string>.Fail(primary, extras);

		response.Errors.Should().HaveCount(3);
	}

	/// <summary>
	/// Verifies that Fail filters out null and Error.None from additional errors.
	/// </summary>
	[Fact]
	public void Fail_ShouldFilterNullAndNoneErrors()
	{
		var primary = new Error("ERR001", "Primary");
		var extras = new Error[] { null!, Error.None, new Error("ERR002", "Valid") };

		var response = ApiResponse<string>.Fail(primary, extras);

		response.Errors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that Fail with custom message uses it.
	/// </summary>
	[Fact]
	public void Fail_WithCustomMessage_ShouldUseIt()
	{
		var error = new Error("ERR001", "Default message");

		var response = ApiResponse<string>.Fail(error, message: "Custom message");

		response.Message.Should().Be("Custom message");
	}
	#endregion

	#region FromResult Tests
	/// <summary>
	/// Verifies that FromResult with success result creates Ok response.
	/// </summary>
	[Fact]
	public void FromResult_SuccessResult_ShouldCreateOkResponse()
	{
		var result = Result.Success("data");

		var response = ApiResponse<string>.FromResult(result);

		response.Success.Should().BeTrue();
		response.Data.Should().Be("data");
	}

	/// <summary>
	/// Verifies that FromResult with success and custom message includes it.
	/// </summary>
	[Fact]
	public void FromResult_SuccessWithMessage_ShouldIncludeMessage()
	{
		var result = Result.Success(42);

		var response = ApiResponse<int>.FromResult(result, "Created successfully");

		response.Message.Should().Be("Created successfully");
	}

	/// <summary>
	/// Verifies that FromResult with failure result creates Fail response.
	/// </summary>
	[Fact]
	public void FromResult_FailureResult_ShouldCreateFailResponse()
	{
		var error = new Error("ERR", "Not found");
		var result = Result.Failure<string>(error);

		var response = ApiResponse<string>.FromResult(result);

		response.Success.Should().BeFalse();
		response.Errors.Should().NotBeEmpty();
	}

	/// <summary>
	/// Verifies that FromResult with ValidationResult includes validation errors.
	/// </summary>
	[Fact]
	public void FromResult_ValidationResult_ShouldIncludeValidationErrors()
	{
		var result = ValidationResult<string>.WithErrors(
			new Error("ERR1", "Error 1"),
			new Error("ERR2", "Error 2"));

		var response = ApiResponse<string>.FromResult(result);

		response.Success.Should().BeFalse();
		response.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
	}

	/// <summary>
	/// Verifies that FromResult with null throws.
	/// </summary>
	[Fact]
	public void FromResult_Null_ShouldThrow()
	{
		var act = () => ApiResponse<string>.FromResult(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Default Values Tests
	/// <summary>
	/// Verifies default property values of ApiResponse.
	/// </summary>
	[Fact]
	public void DefaultProperties_ShouldHaveCorrectValues()
	{
		var response = new ApiResponse<string>();

		response.Success.Should().BeTrue();
		response.Message.Should().BeEmpty();
		response.Errors.Should().BeEmpty();
		response.Data.Should().BeNull();
	}
	#endregion
}
