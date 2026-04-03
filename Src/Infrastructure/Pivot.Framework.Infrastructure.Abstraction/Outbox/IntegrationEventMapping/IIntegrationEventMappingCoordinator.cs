using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Coordinates domain-event-to-integration-event mapping for a specific
///              persistence context. The coordinator resolves all registered mappers,
///              translates domain events, and enqueues the resulting integration events
///              within the current transaction.
/// </summary>
/// <typeparam name="TContext">The persistence context used as a DI discriminator.</typeparam>
public interface IIntegrationEventMappingCoordinator<TContext>
	where TContext : class, IPersistenceContext
{
	/// <summary>
	/// Maps the supplied domain events and enqueues all resulting integration events.
	/// </summary>
	/// <param name="domainEvents">The domain events to translate.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or the first failure encountered.</returns>
	Task<Result> PublishMappedIntegrationEventsAsync(
		IReadOnlyCollection<IDomainEvent> domainEvents,
		CancellationToken cancellationToken = default);
}
