using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for publishing domain events to the outbox.
///              Ensures domain events are persisted before being dispatched to external consumers.
/// </summary>
public interface IDomainEventPublisher
{
	#region Methods

	/// <summary>
	/// Publishes the specified domain event asynchronously.
	/// </summary>
	/// <param name="domainEvent">The domain event to publish.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the publish operation.</returns>
	Task<Result> PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

	#endregion
}
