using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.Inbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Non-generic inbox service abstraction for consumer-side message deduplication.
///              Wraps the context-specific <see cref="Repositories.IInboxRepository{TContext}"/>
///              to allow injection into non-generic components such as <c>RabbitMQReceiver</c>
///              and <c>InProcessMessagePublisher</c>.
///
///              The inbox guarantees idempotent message processing: before dispatching a message,
///              consumers check <see cref="HasBeenProcessedAsync"/> and after successful dispatch
///              call <see cref="RecordConsumptionAsync"/> within the same unit of work.
/// </summary>
public interface IInboxService
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
	/// Records that a message has been processed by the named consumer and persists
	/// the change to the underlying store.
	/// </summary>
	/// <param name="messageId">The unique identifier of the message.</param>
	/// <param name="consumerName">The name of the consumer that processed the message.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> RecordConsumptionAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Persists all pending inbox changes to the underlying store.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	Task SaveChangesAsync(CancellationToken cancellationToken = default);

	#endregion
}
