using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="CustomValidationProblemDetails"/>.
///              Verifies default property values and initialization.
/// </summary>
public class CustomValidationProblemDetailsTests
{
	#region Default Value Tests
	/// <summary>
	/// Verifies that ValidationErrors defaults to empty collection.
	/// </summary>
	[Fact]
	public void Default_ValidationErrors_ShouldBeEmpty()
	{
		var details = new CustomValidationProblemDetails();

		details.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	/// Verifies that it inherits from ProblemDetails.
	/// </summary>
	[Fact]
	public void ShouldInheritFromProblemDetails()
	{
		var details = new CustomValidationProblemDetails();

		details.Should().BeAssignableTo<ProblemDetails>();
	}
	#endregion

	#region Initialization Tests
	/// <summary>
	/// Verifies that ValidationErrors can be initialized with errors.
	/// </summary>
	[Fact]
	public void Init_WithErrors_ShouldStoreErrors()
	{
		var errors = new[] { new Error("ERR1", "Error one"), new Error("ERR2", "Error two") };

		var details = new CustomValidationProblemDetails { ValidationErrors = errors };

		details.ValidationErrors.Should().HaveCount(2);
	}

	/// <summary>
	/// Verifies that ProblemDetails properties are accessible.
	/// </summary>
	[Fact]
	public void ProblemDetailsProperties_ShouldBeAccessible()
	{
		var details = new CustomValidationProblemDetails
		{
			Title = "Validation Error",
			Status = 400,
			Detail = "One or more errors"
		};

		details.Title.Should().Be("Validation Error");
		details.Status.Should().Be(400);
		details.Detail.Should().Be("One or more errors");
	}
	#endregion
}
