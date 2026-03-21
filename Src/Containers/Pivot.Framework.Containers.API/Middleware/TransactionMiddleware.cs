using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Expanded commit range to include HTTP 422 (Unprocessable Entity)
///              so that domain validation responses whose side-effects should be persisted
///              are committed rather than rolled back.
/// Purpose     : Wraps non-GET requests in a database transaction.
///              Transaction ownership is at middleware level (not UnitOfWork).
///              Commits on 2xx success and 422 domain validation responses;
///              rolls back on all other status codes and on unhandled exceptions.
/// </summary>
public sealed class TransactionMiddleware<TContext> where TContext : DbContext
{
	#region Fields
	/// <summary>
	/// The next middleware in the ASP.NET Core pipeline.
	/// </summary>
	private readonly RequestDelegate _next;

	/// <summary>
	/// Logger for diagnostic tracing of transaction lifecycle events.
	/// </summary>
	private readonly ILogger<TransactionMiddleware<TContext>> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="TransactionMiddleware{TContext}"/> with the provided dependencies.
	/// </summary>
	/// <param name="next">The next middleware delegate. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="next"/> or <paramref name="logger"/> is null.
	/// </exception>
	public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware<TContext>> logger)
	{
		_next = next ?? throw new ArgumentNullException(nameof(next));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Invokes the middleware. GET requests bypass the transaction entirely.
	/// All other HTTP methods are wrapped in a database transaction that is committed
	/// on success (2xx) or domain validation (422), and rolled back otherwise.
	/// </summary>
	/// <param name="context">The HTTP context for the current request.</param>
	public async Task InvokeAsync(HttpContext context)
	{
		var transactionManager = context.RequestServices.GetRequiredService<ITransactionManager<TContext>>();

		if (context.Request.Method == HttpMethods.Get)
		{
			await _next(context);
			return;
		}

		await transactionManager.BeginTransactionAsync();

		try
		{
			await _next(context);

			if (ShouldCommit(context.Response.StatusCode))
			{
				await transactionManager.CommitTransactionAsync();
			}
			else
			{
				await transactionManager.RollbackTransactionAsync();
			}
		}
		catch
		{
			await transactionManager.RollbackTransactionAsync();
			throw; // CRITICAL: do not swallow exceptions
		}
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Determines whether the transaction should be committed based on the HTTP status code.
	/// Commits on 2xx (success) and 422 (Unprocessable Entity — domain validation failures
	/// that were processed correctly and whose side-effects should be persisted).
	/// </summary>
	/// <param name="statusCode">The HTTP response status code.</param>
	/// <returns><c>true</c> when the transaction should be committed; otherwise <c>false</c>.</returns>
	private static bool ShouldCommit(int statusCode)
		=> (statusCode >= 200 && statusCode < 300) || statusCode == 422;
	#endregion
}
