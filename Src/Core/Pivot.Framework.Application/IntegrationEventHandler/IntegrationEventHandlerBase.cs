using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Application.IntegrationEventHandler;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Convenience base class for integration event consumer handlers.
///              Subclasses override <see cref="HandleWithResultAsync"/> and do not need
///              to interact with MediatR notification adapter types directly.
/// </summary>
/// <typeparam name="TEvent">The integration event type.</typeparam>
public abstract class IntegrationEventHandlerBase<TEvent> : IIntegrationEventHandler<TEvent>
	where TEvent : IIntegrationEvent
{
	#region Public Methods
	/// <summary>
	/// Handles the integration event with a result.
	/// </summary>
	/// <param name="integrationEvent">The integration event.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A result indicating success or failure.</returns>
	public abstract Task<Result> HandleWithResultAsync(TEvent integrationEvent, CancellationToken cancellationToken);
	#endregion
}
