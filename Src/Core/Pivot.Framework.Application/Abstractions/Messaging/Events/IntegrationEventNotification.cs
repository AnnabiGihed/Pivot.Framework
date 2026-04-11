using MediatR;
using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : MediatR notification adapter that wraps an integration event.
///              This bridges public cross-service integration events to in-process
///              consumer handlers without making the Domain layer reference MediatR.
/// </summary>
/// <typeparam name="TIntegrationEvent">The concrete integration event type.</typeparam>
public sealed class IntegrationEventNotification<TIntegrationEvent> : INotification
	where TIntegrationEvent : IIntegrationEvent
{
	#region Properties
	/// <summary>
	/// Gets the integration event carried by this notification.
	/// </summary>
	public TIntegrationEvent IntegrationEvent { get; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new notification wrapping the specified integration event.
	/// </summary>
	/// <param name="integrationEvent">The integration event to wrap. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="integrationEvent"/> is null.</exception>
	public IntegrationEventNotification(TIntegrationEvent integrationEvent)
	{
		IntegrationEvent = integrationEvent ?? throw new ArgumentNullException(nameof(integrationEvent));
	}
	#endregion
}
