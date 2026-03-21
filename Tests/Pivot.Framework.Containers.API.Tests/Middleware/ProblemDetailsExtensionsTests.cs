using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ProblemDetailsExtensions"/>.
///              Verifies validation error enrichment, fluent chaining, and null guards.
/// </summary>
public class ProblemDetailsExtensionsTests
{
	#region WithValidationErrors Tests
	/// <summary>
	/// Verifies that errors are added to Extensions dictionary.
	/// </summary>
	[Fact]
	public void WithValidationErrors_NonEmpty_ShouldAddToExtensions()
	{
		var problem = new ProblemDetails();
		var errors = new[] { new Error("ERR1", "Error one") };

		problem.WithValidationErrors(errors);

		problem.Extensions.Should().ContainKey("validationErrors");
	}

	/// <summary>
	/// Verifies that empty errors collection does not add to Extensions.
	/// </summary>
	[Fact]
	public void WithValidationErrors_Empty_ShouldNotAddToExtensions()
	{
		var problem = new ProblemDetails();

		problem.WithValidationErrors(Array.Empty<Error>());

		problem.Extensions.Should().NotContainKey("validationErrors");
	}

	/// <summary>
	/// Verifies fluent chaining returns same ProblemDetails instance.
	/// </summary>
	[Fact]
	public void WithValidationErrors_ShouldReturnSameInstance()
	{
		var problem = new ProblemDetails();
		var errors = new[] { new Error("ERR1", "Error one") };

		var result = problem.WithValidationErrors(errors);

		result.Should().BeSameAs(problem);
	}

	/// <summary>
	/// Verifies that null problemDetails throws.
	/// </summary>
	[Fact]
	public void WithValidationErrors_NullProblemDetails_ShouldThrow()
	{
		ProblemDetails problem = null!;
		var errors = new[] { new Error("ERR1", "Error one") };

		var act = () => problem.WithValidationErrors(errors);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null errors throws.
	/// </summary>
	[Fact]
	public void WithValidationErrors_NullErrors_ShouldThrow()
	{
		var problem = new ProblemDetails();

		var act = () => problem.WithValidationErrors(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that multiple errors are stored correctly.
	/// </summary>
	[Fact]
	public void WithValidationErrors_MultipleErrors_ShouldStoreAll()
	{
		var problem = new ProblemDetails();
		var errors = new[]
		{
			new Error("ERR1", "First"),
			new Error("ERR2", "Second"),
			new Error("ERR3", "Third")
		};

		problem.WithValidationErrors(errors);

		var stored = problem.Extensions["validationErrors"] as Error[];
		stored.Should().HaveCount(3);
	}
	#endregion
}
