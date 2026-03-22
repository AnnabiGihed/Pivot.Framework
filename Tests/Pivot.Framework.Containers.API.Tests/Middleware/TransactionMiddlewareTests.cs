using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Containers.API.Middleware;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Containers.API.Tests.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="TransactionMiddleware{TContext}"/>.
///              Verifies that GET requests bypass transactions, 2xx and 422 responses commit,
///              error responses rollback, and exceptions trigger rollback with rethrow.
/// </summary>
public class TransactionMiddlewareTests
{
	#region Fields
	private readonly ITransactionManager<TestDbContext> _transactionManager;
	private readonly ILogger<TransactionMiddleware<TestDbContext>> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises test dependencies with NSubstitute mocks.
	/// </summary>
	public TransactionMiddlewareTests()
	{
		_transactionManager = Substitute.For<ITransactionManager<TestDbContext>>();
		_logger = Substitute.For<ILogger<TransactionMiddleware<TestDbContext>>>();
	}
	#endregion

	#region Bypass Tests
	/// <summary>
	/// Verifies that GET requests bypass the transaction pipeline entirely.
	/// </summary>
	[Fact]
	public async Task GET_Request_ShouldBypassTransaction()
	{
		var middleware = new TransactionMiddleware<TestDbContext>(
			next: _ => Task.CompletedTask, _logger);

		var context = CreateHttpContext("GET", 200);

		await middleware.InvokeAsync(context);

		await _transactionManager.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
	}
	#endregion

	#region Commit Tests
	/// <summary>
	/// Verifies that POST requests resulting in 2xx status codes commit the transaction.
	/// </summary>
	[Theory]
	[InlineData(200)]
	[InlineData(201)]
	[InlineData(204)]
	[InlineData(299)]
	public async Task POST_With2xxStatus_ShouldCommit(int statusCode)
	{
		var middleware = new TransactionMiddleware<TestDbContext>(
			next: ctx => { ctx.Response.StatusCode = statusCode; return Task.CompletedTask; },
			_logger);

		var context = CreateHttpContext("POST", statusCode);

		await middleware.InvokeAsync(context);

		await _transactionManager.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
		await _transactionManager.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that HTTP 422 (Unprocessable Entity) — a domain validation response —
	/// commits the transaction because the business state is valid.
	/// </summary>
	[Fact]
	public async Task POST_With422Status_ShouldCommit()
	{
		var middleware = new TransactionMiddleware<TestDbContext>(
			next: ctx => { ctx.Response.StatusCode = 422; return Task.CompletedTask; },
			_logger);

		var context = CreateHttpContext("POST", 422);

		await middleware.InvokeAsync(context);

		await _transactionManager.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
		await _transactionManager.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
	}
	#endregion

	#region Rollback Tests
	/// <summary>
	/// Verifies that POST requests resulting in error status codes rollback the transaction.
	/// </summary>
	[Theory]
	[InlineData(400)]
	[InlineData(401)]
	[InlineData(403)]
	[InlineData(404)]
	[InlineData(500)]
	public async Task POST_WithErrorStatus_ShouldRollback(int statusCode)
	{
		var middleware = new TransactionMiddleware<TestDbContext>(
			next: ctx => { ctx.Response.StatusCode = statusCode; return Task.CompletedTask; },
			_logger);

		var context = CreateHttpContext("POST", statusCode);

		await middleware.InvokeAsync(context);

		await _transactionManager.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
		await _transactionManager.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that when the delegate throws an exception, the transaction is rolled back
	/// and the exception is re-thrown.
	/// </summary>
	[Fact]
	public async Task POST_WhenExceptionThrown_ShouldRollbackAndRethrow()
	{
		var middleware = new TransactionMiddleware<TestDbContext>(
			next: _ => throw new InvalidOperationException("boom"),
			_logger);

		var context = CreateHttpContext("POST", 200);

		var act = () => middleware.InvokeAsync(context);

		await act.Should().ThrowAsync<InvalidOperationException>();
		await _transactionManager.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Creates an <see cref="HttpContext"/> configured with the specified HTTP method,
	/// status code, and a scoped <see cref="ITransactionManager{TContext}"/>.
	/// </summary>
	/// <param name="method">The HTTP method (GET, POST, etc.).</param>
	/// <param name="statusCode">The initial response status code.</param>
	/// <returns>A configured <see cref="DefaultHttpContext"/> instance.</returns>
	private HttpContext CreateHttpContext(string method, int statusCode)
	{
		var context = new DefaultHttpContext();
		context.Request.Method = method;
		context.Response.StatusCode = statusCode;

		var services = new ServiceCollection();
		services.AddSingleton(_transactionManager);
		context.RequestServices = services.BuildServiceProvider();

		return context;
	}
	#endregion

	#region Test Doubles
	/// <summary>
	/// Minimal <see cref="DbContext"/> test double for the transaction middleware tests.
	/// </summary>
	public class TestDbContext : DbContext, IPersistenceContext
	{
		/// <summary>
		/// Initialises a new <see cref="TestDbContext"/> with the specified options.
		/// </summary>
		/// <param name="options">The EF Core context options.</param>
		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
	}
	#endregion
}
