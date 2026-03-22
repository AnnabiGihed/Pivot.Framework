using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Repository for managing outbox messages.
///              SaveChanges is intentionally NOT called inside repository methods.
/// </summary>
public interface IOutboxRepository<TContext> where TContext : class, IPersistenceContext
{
	#region Methods

	/// <summary>
	/// Adds a new outbox message to the repository asynchronously.
	/// </summary>
	/// <param name="message">The outbox message to add.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the add operation.</returns>
	Task<Result> AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

	/// <summary>
	/// Marks the specified outbox message as processed asynchronously.
	/// </summary>
	/// <param name="messageId">The unique identifier of the message to mark as processed.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the operation.</returns>
	Task<Result> MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all unprocessed outbox messages asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	/// <returns>A read-only list of unprocessed <see cref="OutboxMessage"/> instances.</returns>
	Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(CancellationToken cancellationToken = default);

	#endregion
}
