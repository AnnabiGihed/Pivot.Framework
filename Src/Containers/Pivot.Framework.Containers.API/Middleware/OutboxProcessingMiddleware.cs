using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Processes outbox messages AFTER request completion and transaction commit.
///              Must be registered BEFORE TransactionMiddleware.
/// </summary>
internal sealed class OutboxProcessingMiddleware<TContext> where TContext : DbContext, IPersistenceContext
{
	#region Fields

	/// <summary>
	/// The next middleware in the ASP.NET Core pipeline.
	/// </summary>
	private readonly RequestDelegate _next;

	/// <summary>
	/// Logger for diagnostic output.
	/// </summary>
	private readonly ILogger<OutboxProcessingMiddleware<TContext>> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="OutboxProcessingMiddleware{TContext}"/> with the provided dependencies.
	/// </summary>
	/// <param name="next">The next middleware delegate. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="next"/> or <paramref name="logger"/> is null.
	/// </exception>
	public OutboxProcessingMiddleware(RequestDelegate next, ILogger<OutboxProcessingMiddleware<TContext>> logger)
	{
		_next = next ?? throw new ArgumentNullException(nameof(next));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Invokes the middleware. After the pipeline completes with a 2xx status,
	/// resolves and runs the <see cref="IOutboxProcessor{TContext}"/> to dispatch pending outbox messages.
	/// </summary>
	/// <param name="context">The HTTP context for the current request.</param>
	public async Task InvokeAsync(HttpContext context)
	{
		await _next(context);

		if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300)
			return;

		try
		{
			var processor = context.RequestServices.GetService<IOutboxProcessor<TContext>>();

			if (processor is null)
				return;

			await processor.ProcessOutboxMessagesAsync(context.RequestAborted);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing outbox messages.");
		}
	}

	#endregion
}
