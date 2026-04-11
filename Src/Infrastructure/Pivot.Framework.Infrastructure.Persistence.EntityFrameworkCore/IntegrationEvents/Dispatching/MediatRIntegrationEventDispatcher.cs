using MediatR;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Messaging.Events;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.IntegrationEvents.Dispatching;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : MediatR-based implementation of <see cref="IIntegrationEventDispatcher"/>.
///              Wraps each <see cref="IIntegrationEvent"/> in an <see cref="IntegrationEventNotification{TIntegrationEvent}"/>
///              and publishes it through MediatR's notification pipeline.
/// </summary>
public sealed class MediatRIntegrationEventDispatcher : IIntegrationEventDispatcher
{
	#region Fields
	/// <summary>
	/// The MediatR mediator used for in-process notification publishing.
	/// </summary>
	private readonly IMediator _mediator;

	/// <summary>
	/// Logger for diagnostic tracing of integration event dispatch operations.
	/// </summary>
	private readonly ILogger<MediatRIntegrationEventDispatcher> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="MediatRIntegrationEventDispatcher"/> with the provided dependencies.
	/// </summary>
	/// <param name="mediator">The MediatR mediator. Must not be null.</param>
	/// <param name="logger">The logger instance. Must not be null.</param>
	public MediatRIntegrationEventDispatcher(IMediator mediator, ILogger<MediatRIntegrationEventDispatcher> logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region IIntegrationEventDispatcher Implementation
	/// <inheritdoc />
	public async Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(integrationEvent);

		var eventType = integrationEvent.GetType();
		var notificationType = typeof(IntegrationEventNotification<>).MakeGenericType(eventType);
		var notification = Activator.CreateInstance(notificationType, integrationEvent)
			?? throw new InvalidOperationException($"Failed to create IntegrationEventNotification<{eventType.Name}>.");

		_logger.LogDebug("Dispatching integration event {EventType} ({EventId}) via MediatR.", eventType.Name, integrationEvent.Id);

		await _mediator.Publish(notification, cancellationToken);
	}
	#endregion
}
