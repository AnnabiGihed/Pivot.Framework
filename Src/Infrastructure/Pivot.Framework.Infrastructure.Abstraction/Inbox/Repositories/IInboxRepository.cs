using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Inbox.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Repository contract for the inbox pattern (consumer-side idempotency).
///              Tracks which messages have been processed by which consumer to prevent
///              duplicate handling in at-least-once delivery scenarios.
///              SaveChanges is intentionally NOT called inside repository methods.
/// </summary>
/// <typeparam name="TContext">The persistence context type, used as a DI discriminator.</typeparam>
public interface IInboxRepository<TContext> where TContext : class, IPersistenceContext
{
	#region Methods

	/// <summary>
	/// Checks whether a specific message has already been processed by the named consumer.
	/// </summary>
	/// <param name="messageId">The unique identifier of the message.</param>
	/// <param name="consumerName">The name of the consumer (e.g., service or handler name).</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>True if the message has already been processed by this consumer; otherwise false.</returns>
	Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Records that a message has been processed by the named consumer.
	/// Must be called within the same transaction as the handler's side effects
	/// to ensure atomic inbox-and-effect persistence.
	/// </summary>
	/// <param name="messageId">The unique identifier of the message.</param>
	/// <param name="consumerName">The name of the consumer that processed the message.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> RecordConsumptionAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);

	#endregion
}
