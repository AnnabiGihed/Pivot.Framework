using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Processor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for processing pending outbox messages.
///              Retrieves unprocessed messages and dispatches them to the message broker.
/// </summary>
/// <typeparam name="TContext">The persistence context type, used as a DI discriminator.</typeparam>
public interface IOutboxProcessor<TContext> where TContext : class, IPersistenceContext
{
	#region Methods

	/// <summary>
	/// Processes all pending outbox messages asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the processing operation.</returns>
	public Task<Result> ProcessOutboxMessagesAsync(CancellationToken cancellationToken);

	#endregion
}
