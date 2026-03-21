using FluentAssertions;
using Pivot.Framework.Application.Responses;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Tests.Responses;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BaseCommandResponse"/>.
///              Verifies Ok/Fail factory methods, error conversion, and default values.
/// </summary>
public class BaseCommandResponseTests
{
	#region Ok Tests
	/// <summary>
	/// Verifies that Ok creates a successful response with empty message.
	/// </summary>
	[Fact]
	public void Ok_WithoutMessage_ShouldCreateSuccessResponse()
	{
		var response = BaseCommandResponse.Ok();

		response.Success.Should().BeTrue();
		response.Message.Should().BeEmpty();
		response.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	/// Verifies that Ok with message includes it.
	/// </summary>
	[Fact]
	public void Ok_WithMessage_ShouldIncludeMessage()
	{
		var response = BaseCommandResponse.Ok("Created successfully");

		response.Success.Should().BeTrue();
		response.Message.Should().Be("Created successfully");
	}
	#endregion

	#region Fail Tests
	/// <summary>
	/// Verifies that Fail creates a failure response.
	/// </summary>
	[Fact]
	public void Fail_ShouldCreateFailureResponse()
	{
		var response = BaseCommandResponse.Fail("Something went wrong");

		response.Success.Should().BeFalse();
		response.Message.Should().Be("Something went wrong");
	}

	/// <summary>
	/// Verifies that Fail with errors converts them to strings.
	/// </summary>
	[Fact]
	public void Fail_WithErrors_ShouldConvertToStrings()
	{
		var errors = new[]
		{
			new Error("ERR1", "Name is required"),
			new Error("ERR2", "Email is invalid")
		};

		var response = BaseCommandResponse.Fail("Validation failed", errors);

		response.Success.Should().BeFalse();
		response.ValidationErrors.Should().HaveCount(2);
		response.ValidationErrors.Should().Contain("Name is required");
		response.ValidationErrors.Should().Contain("Email is invalid");
	}

	/// <summary>
	/// Verifies that Fail filters out null and Error.None from errors.
	/// </summary>
	[Fact]
	public void Fail_ShouldFilterNullAndNoneErrors()
	{
		var errors = new Error[]
		{
			new("ERR1", "Valid error"),
			null!,
			Error.None
		};

		var response = BaseCommandResponse.Fail("Failed", errors);

		response.ValidationErrors.Should().HaveCount(1);
		response.ValidationErrors.Should().Contain("Valid error");
	}

	/// <summary>
	/// Verifies that Fail without errors has empty validation errors.
	/// </summary>
	[Fact]
	public void Fail_WithoutErrors_ShouldHaveEmptyValidationErrors()
	{
		var response = BaseCommandResponse.Fail("Error");

		response.ValidationErrors.Should().BeEmpty();
	}
	#endregion
}
