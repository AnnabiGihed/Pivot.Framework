using FluentAssertions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Tests.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ValidationErrors"/>.
///              Verifies the predefined validation error instance.
/// </summary>
public class ValidationErrorsTests
{
	#region Predefined Error Tests
	/// <summary>
	/// Verifies that <see cref="ValidationErrors.ValidationError"/> has expected code and message.
	/// </summary>
	[Fact]
	public void ValidationError_ShouldHaveExpectedCodeAndMessage()
	{
		ValidationErrors.ValidationError.Code.Should().Be("ValidationError");
		ValidationErrors.ValidationError.Message.Should().Be("A validation problem occurred.");
	}

	/// <summary>
	/// Verifies that <see cref="ValidationErrors.ValidationError"/> is not Error.None.
	/// </summary>
	[Fact]
	public void ValidationError_ShouldNotBeNone()
	{
		(ValidationErrors.ValidationError == Error.None).Should().BeFalse();
	}
	#endregion
}
