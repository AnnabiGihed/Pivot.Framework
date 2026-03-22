using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : EF Core implementation of <see cref="IOutboxRepository{TContext}"/>.
///              Provides persistence operations for outbox messages including
///              adding new messages, marking messages as processed, and retrieving
///              unprocessed messages ordered by creation date.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that contains the outbox table.</typeparam>
public sealed class OutboxRepository<TContext>(TContext dbContext) : IOutboxRepository<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Fields
	/// <summary>
	/// The EF Core database context used for outbox message persistence.
	/// </summary>
	private readonly TContext _dbContext = dbContext;
	#endregion

	#region Public Methods
	/// <summary>
	/// Adds a new outbox message to the change tracker.
	/// The message is persisted when the ambient unit of work commits.
	/// </summary>
	/// <param name="message">The outbox message to add.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success.</returns>
	public Task<Result> AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<OutboxMessage>().Add(message);
		return Task.FromResult(Result.Success());
	}

	/// <summary>
	/// Marks an outbox message as processed by setting its <see cref="OutboxMessage.Processed"/>
	/// flag and recording the processing timestamp.
	/// </summary>
	/// <param name="messageId">The identifier of the message to mark as processed.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public async Task<Result> MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
	{
		try
		{
			var message = await _dbContext.Set<OutboxMessage>()
				.FindAsync(new object[] { messageId }, cancellationToken);

			if (message is null)
				return Result.Success();

			message.Processed = true;
			message.ProcessedAtUtc = DateTime.UtcNow;

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("MarkAsProcessedError", ex.Message));
		}
	}

	/// <summary>
	/// Retrieves all unprocessed outbox messages ordered by creation date (oldest first).
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A read-only list of unprocessed <see cref="OutboxMessage"/> instances.</returns>
	public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<OutboxMessage>()
			.Where(m => !m.Processed)
			.OrderBy(m => m.CreatedAtUtc)
			.ToListAsync(cancellationToken);
	}
	#endregion
}
