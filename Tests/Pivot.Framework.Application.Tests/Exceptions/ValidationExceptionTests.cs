using FluentAssertions;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ValidationException"/>.
///              Verifies construction, error normalisation, and message building.
/// </summary>
public class ValidationExceptionTests
{
	#region Single Error Constructor Tests
	/// <summary>
	/// Verifies that the single-error constructor assigns properties correctly.
	/// </summary>
	[Fact]
	public void Constructor_SingleError_ShouldAssignProperties()
	{
		var error = new Error("VAL001", "Name is required");

		var ex = new ValidationException(error);

		ex.PrimaryError.Should().Be(error);
		ex.ValidationErrors.Should().HaveCount(1);
		ex.Message.Should().Be("Name is required");
	}

	/// <summary>
	/// Verifies that message falls back to code when message is whitespace.
	/// </summary>
	[Fact]
	public void Constructor_WhitespaceMessage_ShouldFallbackToCode()
	{
		var error = new Error("VAL001", "   ");

		var ex = new ValidationException(error);

		ex.Message.Should().Be("VAL001");
	}

	/// <summary>
	/// Verifies that null error throws.
	/// </summary>
	[Fact]
	public void Constructor_NullError_ShouldThrow()
	{
		var act = () => new ValidationException(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Validation Result Constructor Tests
	/// <summary>
	/// Verifies that errors from validation result are used.
	/// </summary>
	[Fact]
	public void Constructor_WithValidationResult_ShouldUseValidationErrors()
	{
		var error = new Error("VAL001", "Primary");
		var validationResult = ValidationResult.WithErrors(
			new Error("VAL002", "Error A"),
			new Error("VAL003", "Error B"));

		var ex = new ValidationException(error, validationResult);

		ex.PrimaryError.Should().Be(error);
		ex.ValidationErrors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that null validation result throws.
	/// </summary>
	[Fact]
	public void Constructor_NullValidationResult_ShouldThrow()
	{
		var error = new Error("VAL001", "msg");

		var act = () => new ValidationException(error, null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion
}
