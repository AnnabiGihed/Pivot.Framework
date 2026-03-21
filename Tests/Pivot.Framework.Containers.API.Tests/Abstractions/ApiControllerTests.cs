using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Containers.API.Abstractions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Tests.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ApiController"/>.
///              Verifies HandleFailure, HandleGlobalFailure, and HandleResult
///              across all ResultExceptionType variants.
/// </summary>
public class ApiControllerTests
{
	#region Test Infrastructure
	/// <summary>
	/// Concrete test double exposing protected methods of ApiController.
	/// </summary>
	private class TestApiController : ApiController
	{
		public TestApiController(ISender sender) : base(sender) { }

		public ActionResult InvokeHandleFailure(Result result) => HandleFailure(result);
		public ActionResult InvokeHandleGlobalFailure(Result result) => HandleGlobalFailure(result);
		public ActionResult InvokeHandleResult<T>(Result<T> result) => HandleResult(result);
	}

	private readonly TestApiController _controller;

	public ApiControllerTests()
	{
		var sender = Substitute.For<ISender>();
		_controller = new TestApiController(sender)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			}
		};
	}
	#endregion

	#region Constructor Tests
	/// <summary>
	/// Verifies that null sender throws.
	/// </summary>
	[Fact]
	public void Constructor_NullSender_ShouldThrow()
	{
		var act = () => new TestApiController(null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region HandleFailure Tests
	/// <summary>
	/// Verifies that HandleFailure with successful result throws.
	/// </summary>
	[Fact]
	public void HandleFailure_SuccessResult_ShouldThrow()
	{
		var result = Result.Success();

		var act = () => _controller.InvokeHandleFailure(result);

		act.Should().Throw<InvalidOperationException>();
	}

	/// <summary>
	/// Verifies that HandleFailure with null result throws.
	/// </summary>
	[Fact]
	public void HandleFailure_NullResult_ShouldThrow()
	{
		var act = () => _controller.InvokeHandleFailure(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that NotFound exception type returns 404.
	/// </summary>
	[Fact]
	public void HandleFailure_NotFound_ShouldReturn404()
	{
		var result = Result.Failure(new Error("NotFound", "Resource not found"),
			ResultExceptionType.NotFound);

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
	}

	/// <summary>
	/// Verifies that Conflict exception type returns 409.
	/// </summary>
	[Fact]
	public void HandleFailure_Conflict_ShouldReturn409()
	{
		var result = Result.Failure(new Error("Conflict", "Already exists"),
			ResultExceptionType.Conflict);

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
	}

	/// <summary>
	/// Verifies that Unauthorized exception type returns 401.
	/// </summary>
	[Fact]
	public void HandleFailure_Unauthorized_ShouldReturn401()
	{
		var result = Result.Failure(new Error("Unauthorized", "Not authenticated"),
			ResultExceptionType.Unauthorized);

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
	}

	/// <summary>
	/// Verifies that Forbidden exception type returns 403.
	/// </summary>
	[Fact]
	public void HandleFailure_Forbidden_ShouldReturn403()
	{
		var result = Result.Failure(new Error("Forbidden", "Access denied"),
			ResultExceptionType.Forbidden);

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
	}

	/// <summary>
	/// Verifies that default (BadRequest) exception type returns 400.
	/// </summary>
	[Fact]
	public void HandleFailure_Default_ShouldReturn400()
	{
		var result = Result.Failure(new Error("BadRequest", "Invalid input"));

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
	}

	/// <summary>
	/// Verifies that HandleFailure returns ProblemDetails with correct title and detail.
	/// </summary>
	[Fact]
	public void HandleFailure_ShouldReturnProblemDetails()
	{
		var error = new Error("Test.Code", "Test message");
		var result = Result.Failure(error);

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		var problemDetails = objectResult!.Value as ProblemDetails;
		problemDetails.Should().NotBeNull();
		problemDetails!.Type.Should().Be("Test.Code");
		problemDetails.Detail.Should().Be("Test message");
	}

	/// <summary>
	/// Verifies that ValidationResult failure includes validation errors in ProblemDetails.
	/// </summary>
	[Fact]
	public void HandleFailure_ValidationResult_ShouldIncludeValidationErrors()
	{
		var result = ValidationResult.WithErrors(
			new Error("VAL1", "Error 1"),
			new Error("VAL2", "Error 2"));

		var actionResult = _controller.InvokeHandleFailure(result);

		var objectResult = actionResult as ObjectResult;
		var problemDetails = objectResult!.Value as ProblemDetails;
		problemDetails.Should().NotBeNull();
		problemDetails!.Title.Should().Be("Validation Error");
		problemDetails.Extensions.Should().ContainKey("validationErrors");
	}
	#endregion

	#region HandleGlobalFailure Tests
	/// <summary>
	/// Verifies that HandleGlobalFailure with successful result throws InvalidOperationException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_SuccessResult_ShouldThrow()
	{
		var result = Result.Success();

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<InvalidOperationException>();
	}

	/// <summary>
	/// Verifies that HandleGlobalFailure with null result throws.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_NullResult_ShouldThrow()
	{
		var act = () => _controller.InvokeHandleGlobalFailure(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that NotFound exception type throws NotFoundException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_NotFound_ShouldThrowNotFoundException()
	{
		var result = Result.Failure(new Error("Order", "123"),
			ResultExceptionType.NotFound);

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<NotFoundException>();
	}

	/// <summary>
	/// Verifies that Conflict exception type throws BadRequestException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_Conflict_ShouldThrowBadRequestException()
	{
		var result = Result.Failure(new Error("Conflict", "Already exists"),
			ResultExceptionType.Conflict);

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<BadRequestException>();
	}

	/// <summary>
	/// Verifies that Unauthorized exception type throws BadRequestException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_Unauthorized_ShouldThrowBadRequestException()
	{
		var result = Result.Failure(new Error("Unauthorized", "Not authenticated"),
			ResultExceptionType.Unauthorized);

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<BadRequestException>();
	}

	/// <summary>
	/// Verifies that Forbidden exception type throws BadRequestException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_Forbidden_ShouldThrowBadRequestException()
	{
		var result = Result.Failure(new Error("Forbidden", "Access denied"),
			ResultExceptionType.Forbidden);

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<BadRequestException>();
	}

	/// <summary>
	/// Verifies that default exception type throws BadRequestException.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_Default_ShouldThrowBadRequestException()
	{
		var result = Result.Failure(new Error("BadRequest", "Invalid input"));

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<BadRequestException>();
	}

	/// <summary>
	/// Verifies that ValidationResult failure throws BadRequestException with validation context.
	/// </summary>
	[Fact]
	public void HandleGlobalFailure_ValidationResult_ShouldThrowBadRequestWithValidation()
	{
		var result = ValidationResult.WithErrors(
			new Error("VAL1", "Error 1"),
			new Error("VAL2", "Error 2"));

		var act = () => _controller.InvokeHandleGlobalFailure(result);

		act.Should().Throw<BadRequestException>();
	}
	#endregion

	#region HandleResult Tests
	/// <summary>
	/// Verifies that successful result returns Ok with value.
	/// </summary>
	[Fact]
	public void HandleResult_Success_ShouldReturnOkWithValue()
	{
		var result = Result.Create("Hello");

		var actionResult = _controller.InvokeHandleResult(result);

		var okResult = actionResult as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult!.Value.Should().Be("Hello");
	}

	/// <summary>
	/// Verifies that failed result delegates to HandleFailure.
	/// </summary>
	[Fact]
	public void HandleResult_Failure_ShouldReturnProblemDetails()
	{
		var result = Result.Failure<string>(new Error("ERR", "Failed"));

		var actionResult = _controller.InvokeHandleResult(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
	}

	/// <summary>
	/// Verifies that null result throws.
	/// </summary>
	[Fact]
	public void HandleResult_NullResult_ShouldThrow()
	{
		var act = () => _controller.InvokeHandleResult<string>(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that HandleResult with NotFound failure returns 404.
	/// </summary>
	[Fact]
	public void HandleResult_NotFoundFailure_ShouldReturn404()
	{
		var result = Result.Failure<string>(
			new Error("NotFound", "Not found"), ResultExceptionType.NotFound);

		var actionResult = _controller.InvokeHandleResult(result);

		var objectResult = actionResult as ObjectResult;
		objectResult.Should().NotBeNull();
		objectResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
	}
	#endregion
}
