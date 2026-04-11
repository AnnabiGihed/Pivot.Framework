using MediatR;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Contract for integration event consumer handlers compatible with MediatR notifications,
///              while allowing handlers to return a <see cref="Result"/> for diagnostics.
/// </summary>
/// <typeparam name="TEvent">The integration event type.</typeparam>
public interface IIntegrationEventHandler<TEvent> : INotificationHandler<IntegrationEventNotification<TEvent>>
	where TEvent : IIntegrationEvent
{
	/// <summary>
	/// Handles the integration event and returns a <see cref="Result"/>.
	/// </summary>
	/// <param name="integrationEvent">The integration event to handle.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A result indicating success or failure.</returns>
	Task<Result> HandleWithResultAsync(TEvent integrationEvent, CancellationToken cancellationToken);

	/// <summary>
	/// MediatR entry point. Unwraps the notification and delegates to <see cref="HandleWithResultAsync"/>.
	/// </summary>
	/// <param name="notification">The integration event notification.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	async Task INotificationHandler<IntegrationEventNotification<TEvent>>.Handle(IntegrationEventNotification<TEvent> notification, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notification);

		_ = await HandleWithResultAsync(notification.IntegrationEvent, cancellationToken);
	}
}
