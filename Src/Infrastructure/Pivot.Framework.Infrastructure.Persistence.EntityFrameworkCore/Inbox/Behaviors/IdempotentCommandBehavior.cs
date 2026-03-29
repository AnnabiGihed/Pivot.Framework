using MediatR;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Application.Abstractions.Messaging.Commands;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox.Behaviors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : MediatR pipeline behavior that provides automatic idempotency for commands
///              implementing <see cref="IIdempotentCommand"/>.
///
///              Before the handler executes, the behavior checks the inbox to determine
///              whether a command with the same <see cref="IIdempotentCommand.IdempotencyKey"/>
///              has already been processed. If so, it short-circuits with a success result.
///              After successful handler execution, it records the consumption in the inbox
///              to prevent duplicate processing on retry or redelivery.
///
///              This behavior only activates for requests that implement both
///              <see cref="ICommand"/> (or <see cref="ICommand{T}"/>) and
///              <see cref="IIdempotentCommand"/>. All other requests pass through unchanged.
///
///              Registration:
///              <code>
///              services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(IdempotentCommandBehavior&lt;,&gt;));
///              </code>
/// </summary>
/// <typeparam name="TRequest">The command request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be assignable from <see cref="Result"/>).</typeparam>
public sealed class IdempotentCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : Result
{
	#region Fields

	private readonly IInboxService? _inboxService;
	private readonly ILogger<IdempotentCommandBehavior<TRequest, TResponse>> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="IdempotentCommandBehavior{TRequest,TResponse}"/>.
	/// </summary>
	/// <param name="logger">Logger for diagnostic tracing.</param>
	/// <param name="inboxService">
	/// The inbox service for deduplication. May be null if inbox support is not registered,
	/// in which case the behavior passes through without deduplication.
	/// </param>
	public IdempotentCommandBehavior(
		ILogger<IdempotentCommandBehavior<TRequest, TResponse>> logger,
		IInboxService? inboxService = null)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_inboxService = inboxService;
	}

	#endregion

	#region IPipelineBehavior Implementation

	/// <inheritdoc />
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		// Only apply idempotency to commands that opt in via IIdempotentCommand.
		if (request is not IIdempotentCommand idempotentCommand || _inboxService is null)
		{
			return await next();
		}

		var consumerName = typeof(TRequest).Name;
		var idempotencyKey = idempotentCommand.IdempotencyKey;

		// ── Check inbox for prior processing ────────────────────────────
		var alreadyProcessed = await _inboxService.HasBeenProcessedAsync(
			idempotencyKey, consumerName, cancellationToken);

		if (alreadyProcessed)
		{
			_logger.LogInformation(
				"Command {CommandType} with idempotency key {IdempotencyKey} has already been processed. Returning success.",
				consumerName, idempotencyKey);

			// Return a success result without re-executing the handler.
			return (TResponse)Result.Success();
		}

		// ── Execute the handler ─────────────────────────────────────────
		var response = await next();

		// ── Record consumption on success ───────────────────────────────
		// Only record if the handler succeeded to allow retries on failure.
		if (response.IsSuccess)
		{
			await _inboxService.RecordConsumptionAsync(idempotencyKey, consumerName, cancellationToken);
			await _inboxService.SaveChangesAsync(cancellationToken);

			_logger.LogDebug(
				"Recorded idempotent consumption for command {CommandType} with key {IdempotencyKey}.",
				consumerName, idempotencyKey);
		}

		return response;
	}

	#endregion
}
