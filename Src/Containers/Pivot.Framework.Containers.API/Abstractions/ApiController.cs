using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstract base controller for all API controllers.
///              Provides common failure-handling helpers that map
///              <see cref="Result"/> outcomes to appropriate HTTP responses
///              (ProblemDetails, exceptions, or typed results).
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
	#region Fields

	/// <summary>
	/// MediatR sender used to dispatch commands and queries.
	/// </summary>
	protected readonly ISender Sender;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="ApiController"/> with the provided MediatR sender.
	/// </summary>
	/// <param name="sender">The MediatR sender instance. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null.</exception>
	protected ApiController(ISender sender) =>
		Sender = sender ?? throw new ArgumentNullException(nameof(sender));

	#endregion

	#region Option 1 : HandleFailure (returns ProblemDetails directly)

	/// <summary>
	/// Maps a failed <see cref="Result"/> to the appropriate HTTP error response
	/// using <see cref="ProblemDetails"/>.
	/// </summary>
	/// <param name="result">The failed result to handle.</param>
	/// <returns>An <see cref="ActionResult"/> containing a <see cref="ProblemDetails"/> payload.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the result is successful.</exception>
	protected ActionResult HandleFailure(Result result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (result.IsSuccess)
			throw new InvalidOperationException("HandleFailure cannot be called for a successful result.");

		return result.ResultExceptionType switch
		{
			ResultExceptionType.NotFound => HandleNotFoundFailure(result),
			ResultExceptionType.Conflict => HandleConflictFailure(result),
			ResultExceptionType.AuthenticationRequired => HandleUnauthorizedFailure(result),
			ResultExceptionType.AccessDenied => HandleForbiddenFailure(result),
			_ => HandleBadRequestFailure(result)
		};
	}

	private ActionResult HandleNotFoundFailure(Result result) =>
		NotFound(CreateProblemDetails(
			title: "Not Found",
			status: StatusCodes.Status404NotFound,
			error: result.Error,
			validationErrors: (result as IValidationResult)?.Errors));

	private ActionResult HandleConflictFailure(Result result) =>
		Conflict(CreateProblemDetails(
			title: "Conflict",
			status: StatusCodes.Status409Conflict,
			error: result.Error,
			validationErrors: (result as IValidationResult)?.Errors));

	private ActionResult HandleUnauthorizedFailure(Result result) =>
		Unauthorized(CreateProblemDetails(
			title: "Unauthorized",
			status: StatusCodes.Status401Unauthorized,
			error: result.Error,
			validationErrors: (result as IValidationResult)?.Errors));

	private ActionResult HandleForbiddenFailure(Result result) =>
		StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails(
			title: "Forbidden",
			status: StatusCodes.Status403Forbidden,
			error: result.Error,
			validationErrors: (result as IValidationResult)?.Errors));

	private ActionResult HandleBadRequestFailure(Result result) =>
		BadRequest(CreateProblemDetails(
			title: result is IValidationResult ? "Validation Error" : "Bad Request",
			status: StatusCodes.Status400BadRequest,
			error: result.Error,
			validationErrors: (result as IValidationResult)?.Errors));

	private static ProblemDetails CreateProblemDetails(
		string title,
		int status,
		Error error,
		IReadOnlyCollection<Error>? validationErrors = null)
	{
		ArgumentNullException.ThrowIfNull(error);

		var problem = new ProblemDetails
		{
			Title = title,
			Status = status,
			Type = error.Code,
			Detail = error.Message
		};

		if (validationErrors is not null && validationErrors.Count > 0)
			problem.Extensions[nameof(validationErrors)] = validationErrors;

		return problem;
	}

	#endregion

	#region Option 2 : HandleGlobalFailure (throws exceptions for middleware)

	/// <summary>
	/// Maps a failed <see cref="Result"/> to a strongly-typed exception
	/// intended to be caught by global exception-handling middleware.
	/// </summary>
	/// <param name="result">The failed result to handle.</param>
	/// <returns>Does not return; always throws.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the result is successful.</exception>
	protected ActionResult HandleGlobalFailure(Result result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (result.IsSuccess)
			throw new InvalidOperationException("HandleGlobalFailure cannot be called for a successful result.");

		// If you use global exception middleware, throw strongly typed exceptions here.
		return result.ResultExceptionType switch
		{
			ResultExceptionType.NotFound => throw CreateNotFoundException(result),
			ResultExceptionType.Conflict => throw new BadRequestException(result.Error), // or create a ConflictException if you want
			ResultExceptionType.AuthenticationRequired => throw new BadRequestException(result.Error), // or UnauthorizedException
			ResultExceptionType.AccessDenied => throw new BadRequestException(result.Error), // or ForbiddenException
			_ => throw CreateBadRequestException(result)
		};
	}

	private static Exception CreateNotFoundException(Result result)
	{
		// Your NotFoundException expects (name, key). We do not have a natural "key" here.
		// Best we can do is map name=error.Code and key=error.Message (or error.Code again).
		// Prefer a richer NotFoundException signature if you want strong typing.
		return new NotFoundException(result.Error.Code, result.Error.Message);
	}

	private static Exception CreateBadRequestException(Result result)
	{
		return result is IValidationResult validationResult
			? new BadRequestException(result.Error, validationResult)
			: new BadRequestException(result.Error);
	}

	#endregion

	#region Option 3 : HandleResult (generic helper for controllers)

	/// <summary>
	/// Convenience method that returns <see cref="OkObjectResult"/> on success
	/// or delegates to <see cref="HandleFailure"/> on failure.
	/// </summary>
	/// <typeparam name="T">The result value type.</typeparam>
	/// <param name="result">The result to evaluate.</param>
	/// <returns>An <see cref="ActionResult"/> representing the outcome.</returns>
	protected ActionResult HandleResult<T>(Result<T> result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (result.IsFailure)
			return HandleFailure(result);

		// Usually you return the value, not the Result wrapper.
		return Ok(result.Value);
	}

	#endregion
}
