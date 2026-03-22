using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.ReadModels;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.Projections;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Resolves and invokes <b>only</b> <see cref="IProjectionHandler{TEvent}"/>
///              implementations for a given domain event.
///
///              Unlike <see cref="Application.Abstractions.Messaging.Events.IDomainEventDispatcher"/>
///              (which publishes to ALL MediatR handlers including side-effect handlers like
///              email senders and notification services), this dispatcher targets projection
///              handlers exclusively — no side effects leak.
///
///              Uses reflection to construct <c>IProjectionHandler&lt;TEvent&gt;</c> at runtime
///              (since the event type is only known at runtime during replay scenarios).
/// </summary>
public class ProjectionDispatcher : IProjectionDispatcher
{
	#region Fields
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ProjectionDispatcher> _logger;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="ProjectionDispatcher"/>.
	/// </summary>
	/// <param name="serviceProvider">The service provider for resolving projection handlers.</param>
	/// <param name="logger">The logger instance.</param>
	public ProjectionDispatcher(
		IServiceProvider serviceProvider,
		ILogger<ProjectionDispatcher> logger)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}
	#endregion

	#region IProjectionDispatcher Implementation
	/// <summary>
	/// Dispatches the given <paramref name="domainEvent"/> to all registered
	/// <see cref="IProjectionHandler{TEvent}"/> instances for the event's runtime type.
	/// No side-effect handlers are invoked.
	/// </summary>
	/// <param name="domainEvent">The domain event to dispatch. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(domainEvent);

		var eventType = domainEvent.GetType();
		var handlerInterfaceType = typeof(IProjectionHandler<>).MakeGenericType(eventType);
		var handlers = _serviceProvider.GetServices(handlerInterfaceType);

		var handlerCount = 0;
		var exceptions = new List<Exception>();
		foreach (var handler in handlers)
		{
			if (handler is null) continue;

			try
			{
				var projectMethod = handlerInterfaceType.GetMethod(
					nameof(IProjectionHandler<IDomainEvent>.ProjectAsync));

				if (projectMethod is null)
				{
					_logger.LogWarning(
						"ProjectAsync method not found on handler {HandlerType} for event {EventType}.",
						handler.GetType().Name, eventType.Name);
					continue;
				}

				var task = (Task)projectMethod.Invoke(handler, new object[] { domainEvent, ct })!;
				await task;
				handlerCount++;
			}
			catch (TargetInvocationException tie) when (tie.InnerException is not null)
			{
				_logger.LogError(tie.InnerException, "Projection handler {Handler} failed for {EventType}.",
					handler.GetType().Name, eventType.Name);
				exceptions.Add(tie.InnerException);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Projection handler {Handler} failed for {EventType}.",
					handler.GetType().Name, eventType.Name);
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count > 0)
			throw new AggregateException($"One or more projection handlers failed for {eventType.Name}.", exceptions);

		_logger.LogDebug(
			"Dispatched {EventType} ({EventId}) to {HandlerCount} projection handler(s).",
			eventType.Name, domainEvent.Id, handlerCount);
	}
	#endregion
}
