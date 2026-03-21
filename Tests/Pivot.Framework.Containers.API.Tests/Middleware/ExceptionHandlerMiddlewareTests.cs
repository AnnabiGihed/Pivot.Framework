using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ExceptionHandlerMiddleware"/>.
///              Verifies exception-to-ProblemDetails mapping for ValidationException,
///              BadRequestException, NotFoundException, and unhandled exceptions.
///              Tests detail exposure in production vs non-production environments.
/// </summary>
public class ExceptionHandlerMiddlewareTests
{
	#region Fields
	private readonly ILogger<ExceptionHandlerMiddleware> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises test dependencies with NSubstitute mocks.
	/// </summary>
	public ExceptionHandlerMiddlewareTests()
	{
		_logger = Substitute.For<ILogger<ExceptionHandlerMiddleware>>();
	}
	#endregion

	#region Success Tests
	/// <summary>
	/// Verifies that when no exception is thrown, the middleware passes through.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_NoException_ShouldPassThrough()
	{
		var environment = CreateEnvironment("Development");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => Task.CompletedTask, _logger, environment);

		var context = new DefaultHttpContext();

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be(200);
	}
	#endregion

	#region ValidationException Tests
	/// <summary>
	/// Verifies that <see cref="ValidationException"/> maps to HTTP 400 Bad Request.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_ValidationException_ShouldReturn400()
	{
		var environment = CreateEnvironment("Development");
		var error = new Error("Validation.Error", "Name is required");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => throw new ValidationException(error),
			_logger, environment);

		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
		context.Response.ContentType.Should().Contain("application/json");
	}
	#endregion

	#region BadRequestException Tests
	/// <summary>
	/// Verifies that <see cref="BadRequestException"/> maps to HTTP 400 Bad Request.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_BadRequestException_ShouldReturn400()
	{
		var environment = CreateEnvironment("Development");
		var error = new Error("Bad.Request", "Invalid input");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => throw new BadRequestException(error),
			_logger, environment);

		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
	}
	#endregion

	#region NotFoundException Tests
	/// <summary>
	/// Verifies that <see cref="NotFoundException"/> maps to HTTP 404 Not Found.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_NotFoundException_ShouldReturn404()
	{
		var environment = CreateEnvironment("Development");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => throw new NotFoundException("Order", Guid.NewGuid()),
			_logger, environment);

		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
	}
	#endregion

	#region Unhandled Exception Tests
	/// <summary>
	/// Verifies that unhandled exceptions map to HTTP 500 Internal Server Error.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_UnhandledException_ShouldReturn500()
	{
		var environment = CreateEnvironment("Development");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => throw new InvalidOperationException("Unexpected"),
			_logger, environment);

		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
	}
	#endregion

	#region Environment Detail Tests
	/// <summary>
	/// Verifies that in production, exception details are NOT exposed in the response.
	/// </summary>
	[Fact]
	public async Task InvokeAsync_InProduction_ShouldNotExposeDetails()
	{
		var environment = CreateEnvironment("Production");
		var middleware = new ExceptionHandlerMiddleware(
			next: _ => throw new InvalidOperationException("Secret internal error"),
			_logger, environment);

		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();

		await middleware.InvokeAsync(context);

		context.Response.Body.Seek(0, SeekOrigin.Begin);
		var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

		body.Should().NotContain("Secret internal error");
	}
	#endregion

	#region Constructor Guard Tests
	/// <summary>
	/// Verifies that null next delegate throws.
	/// </summary>
	[Fact]
	public void Constructor_NullNext_ShouldThrow()
	{
		var act = () => new ExceptionHandlerMiddleware(
			null!, _logger, CreateEnvironment("Development"));

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null logger throws.
	/// </summary>
	[Fact]
	public void Constructor_NullLogger_ShouldThrow()
	{
		var act = () => new ExceptionHandlerMiddleware(
			_ => Task.CompletedTask, null!, CreateEnvironment("Development"));

		act.Should().Throw<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null environment throws.
	/// </summary>
	[Fact]
	public void Constructor_NullEnvironment_ShouldThrow()
	{
		var act = () => new ExceptionHandlerMiddleware(
			_ => Task.CompletedTask, _logger, null!);

		act.Should().Throw<ArgumentNullException>();
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Creates a mock <see cref="IHostEnvironment"/> with the specified environment name.
	/// </summary>
	private static IHostEnvironment CreateEnvironment(string environmentName)
	{
		var env = Substitute.For<IHostEnvironment>();
		env.EnvironmentName.Returns(environmentName);
		return env;
	}
	#endregion
}
