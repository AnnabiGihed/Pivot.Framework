using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Abstraction for dispatching integration events to in-process consumer handlers.
///              This is intentionally separate from <see cref="IDomainEventDispatcher"/> so
///              services can consume public cross-service events without blurring domain and
///              integration event boundaries.
/// </summary>
public interface IIntegrationEventDispatcher
{
	/// <summary>
	/// Dispatches an integration event to all registered handlers.
	/// </summary>
	/// <param name="integrationEvent">The integration event to dispatch.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A task representing the asynchronous dispatch operation.</returns>
	Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
