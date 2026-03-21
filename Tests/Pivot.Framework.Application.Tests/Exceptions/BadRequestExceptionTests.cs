using FluentAssertions;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Tests.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BadRequestException"/>.
///              Verifies construction, error normalisation, and message building.
/// </summary>
public class BadRequestExceptionTests
{
	#region Single Error Constructor Tests
	/// <summary>
	/// Verifies that the single-error constructor assigns properties correctly.
	/// </summary>
	[Fact]
	public void Constructor_SingleError_ShouldAssignProperties()
	{
		var error = new Error("ERR001", "Bad input");

		var ex = new BadRequestException(error);

		ex.PrimaryError.Should().Be(error);
		ex.ValidationErrors.Should().HaveCount(1);
		ex.ValidationErrors.Should().Contain(error);
		ex.Message.Should().Be("Bad input");
	}

	/// <summary>
	/// Verifies that message falls back to code when message is empty.
	/// </summary>
	[Fact]
	public void Constructor_EmptyMessage_ShouldFallbackToCode()
	{
		var error = new Error("ERR001", "");

		var ex = new BadRequestException(error);

		ex.Message.Should().Be("ERR001");
	}

	/// <summary>
	/// Verifies that null error throws.
	/// </summary>
	[Fact]
	public void Constructor_NullError_ShouldThrow()
	{
		var act = () => new BadRequestException(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Validation Result Constructor Tests
	/// <summary>
	/// Verifies that errors from validation result are normalised.
	/// </summary>
	[Fact]
	public void Constructor_WithValidationResult_ShouldNormaliseErrors()
	{
		var error = new Error("ERR001", "Primary");
		var validationResult = ValidationResult.WithErrors(
			new Error("ERR002", "Second"),
			new Error("ERR003", "Third"));

		var ex = new BadRequestException(error, validationResult);

		ex.PrimaryError.Should().Be(error);
		ex.ValidationErrors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that null validation result throws.
	/// </summary>
	[Fact]
	public void Constructor_NullValidationResult_ShouldThrow()
	{
		var error = new Error("ERR001", "msg");

		var act = () => new BadRequestException(error, null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Inheritance Tests
	/// <summary>
	/// Verifies that BadRequestException inherits from Exception.
	/// </summary>
	[Fact]
	public void BadRequestException_ShouldInheritFromException()
	{
		var ex = new BadRequestException(new Error("ERR", "msg"));

		ex.Should().BeAssignableTo<Exception>();
	}
	#endregion
}
