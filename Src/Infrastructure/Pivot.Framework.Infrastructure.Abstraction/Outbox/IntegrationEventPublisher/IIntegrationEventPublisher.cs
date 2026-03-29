using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Abstraction for publishing integration events to the outbox.
///              Integration events are cross-service boundary events that are always
///              routed to the external message broker (never dispatched in-process only).
///              Unlike <see cref="DomainEventPublisher.IDomainEventPublisher"/>, messages
///              persisted by this publisher are stamped with <see cref="Models.MessageKind.IntegrationEvent"/>.
/// </summary>
public interface IIntegrationEventPublisher
{
	#region Methods

	/// <summary>
	/// Serializes an integration event and stores it in the outbox within the current transaction.
	/// The message is stamped with <see cref="Models.MessageKind.IntegrationEvent"/> to ensure
	/// it is always routed to the external broker by the outbox processor.
	/// </summary>
	/// <param name="integrationEvent">The integration event to persist. Must not be null.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	Task<Result> PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken);

	#endregion
}
