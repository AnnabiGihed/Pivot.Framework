using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Global exception handler middleware.
///              Maps application exceptions to RFC 7807 ProblemDetails responses,
///              including validation errors when applicable.
/// </summary>
public sealed class ExceptionHandlerMiddleware
{
	#region Fields

	/// <summary>
	/// The next middleware in the ASP.NET Core pipeline.
	/// </summary>
	private readonly RequestDelegate _next;

	/// <summary>
	/// Logger for diagnostic output.
	/// </summary>
	private readonly ILogger<ExceptionHandlerMiddleware> _logger;

	/// <summary>
	/// Hosting environment used to control detail exposure in responses.
	/// </summary>
	private readonly IHostEnvironment _environment;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="ExceptionHandlerMiddleware"/> with the provided dependencies.
	/// </summary>
	/// <param name="next">The next middleware delegate. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	/// <param name="environment">The hosting environment. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="next"/>, <paramref name="logger"/>, or <paramref name="environment"/> is null.
	/// </exception>
	public ExceptionHandlerMiddleware(
		RequestDelegate next,
		ILogger<ExceptionHandlerMiddleware> logger,
		IHostEnvironment environment)
	{
		_next = next ?? throw new ArgumentNullException(nameof(next));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_environment = environment ?? throw new ArgumentNullException(nameof(environment));
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Invokes the middleware. Catches unhandled exceptions and writes a
	/// <see cref="ProblemDetails"/> JSON response.
	/// </summary>
	/// <param name="httpContext">The HTTP context for the current request.</param>
	public async Task InvokeAsync(HttpContext httpContext)
	{
		try
		{
			await _next(httpContext);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(httpContext, ex);
		}
	}

	#endregion

	#region Private Helpers

	private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
	{
		if (httpContext.Response.HasStarted)
		{
			_logger.LogWarning(ex, "The response has already started; the exception handler will not execute.");
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
		}

		var (statusCode, type, title, errors) = MapException(ex);

		var problem = new ProblemDetails
		{
			Status = (int)statusCode,
			Type = type,
			Title = title,
			Detail = BuildDetail(ex),
			Instance = httpContext.Request.Path
		};

		// Attach traceId for correlation
		problem.Extensions["traceId"] = httpContext.TraceIdentifier;

		// Attach validation errors when present
		if (errors is not null && errors.Count > 0)
			problem.Extensions["validationErrors"] = errors;

		httpContext.Response.Clear();
		httpContext.Response.StatusCode = (int)statusCode;
		httpContext.Response.ContentType = "application/problem+json";

		// Log with the exception object so we keep stack trace in logs
		_logger.LogError(ex,
			"Unhandled exception mapped to {StatusCode}. TraceId: {TraceId}. Type: {Type}. Title: {Title}.",
			(int)statusCode,
			httpContext.TraceIdentifier,
			type,
			title);

		await httpContext.Response.WriteAsJsonAsync(problem);
	}

	private (HttpStatusCode StatusCode, string Type, string Title, IReadOnlyCollection<Error>? Errors) MapException(Exception ex)
	{
		switch (ex)
		{
			case ValidationException validationException:
				return (
					HttpStatusCode.BadRequest,
					nameof(ValidationException),
					validationException.Message,
					validationException.ValidationErrors
				);

			case BadRequestException badRequestException:
				return (
					HttpStatusCode.BadRequest,
					nameof(BadRequestException),
					badRequestException.Message,
					badRequestException.ValidationErrors
				);

			case NotFoundException notFoundException:
				return (
					HttpStatusCode.NotFound,
					nameof(NotFoundException),
					notFoundException.Message,
					null
				);

			default:
				// Do not leak internal details to clients
				return (
					HttpStatusCode.InternalServerError,
					"InternalServerError",
					"An unexpected error occurred.",
					null
				);
		}
	}

	private string? BuildDetail(Exception ex)
	{
		// In production: do not expose stack trace or internal exception details.
		if (_environment.IsProduction())
			return null;

		// In non-production: provide more context for debugging.
		return ex.ToString();
	}

	#endregion
}
