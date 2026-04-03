using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventMapping;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.IntegrationEventMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Resolves all matching domain-event-to-integration-event mappers for the
///              supplied domain events and enqueues the mapped integration events through
///              the integration event publisher within the current transaction.
/// </summary>
/// <typeparam name="TContext">The persistence context used as a DI discriminator.</typeparam>
public sealed class IntegrationEventMappingCoordinator<TContext> : IIntegrationEventMappingCoordinator<TContext>
	where TContext : class, IPersistenceContext
{
	#region Fields
	private static readonly Dictionary<Type, MethodInfo> MapMethodCache = new();
	private static readonly Lock CacheLock = new();

	private readonly IServiceProvider _serviceProvider;
	private readonly IIntegrationEventPublisher<TContext> _integrationEventPublisher;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="IntegrationEventMappingCoordinator{TContext}"/>.
	/// </summary>
	public IntegrationEventMappingCoordinator(
		IServiceProvider serviceProvider,
		IIntegrationEventPublisher<TContext> integrationEventPublisher)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_integrationEventPublisher = integrationEventPublisher ?? throw new ArgumentNullException(nameof(integrationEventPublisher));
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public async Task<Result> PublishMappedIntegrationEventsAsync(
		IReadOnlyCollection<IDomainEvent> domainEvents,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(domainEvents);

		foreach (var domainEvent in domainEvents)
		{
			ArgumentNullException.ThrowIfNull(domainEvent);

			var mapperType = typeof(IIntegrationEventMapper<>).MakeGenericType(domainEvent.GetType());
			var mappers = _serviceProvider.GetServices(mapperType).Cast<object>().ToList();

			if (mappers.Count == 0)
				continue;

			foreach (var mapper in mappers)
			{
				IEnumerable<IIntegrationEvent> integrationEvents;
				try
				{
					integrationEvents = InvokeMap(mapperType, mapper, domainEvent);
				}
				catch (TargetInvocationException ex)
				{
					var message = ex.InnerException?.Message ?? ex.Message;
					return Result.Failure(new Error("IntegrationEventMappingError", message));
				}
				catch (Exception ex)
				{
					return Result.Failure(new Error("IntegrationEventMappingError", ex.Message));
				}

				foreach (var integrationEvent in integrationEvents)
				{
					if (integrationEvent is null)
						return Result.Failure(new Error("IntegrationEventMappingError", "Integration event mapper returned a null integration event."));

					var result = await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
					if (result.IsFailure)
						return result;
				}
			}
		}

		return Result.Success();
	}
	#endregion

	#region Private Helpers
	private static IEnumerable<IIntegrationEvent> InvokeMap(Type mapperType, object mapper, IDomainEvent domainEvent)
	{
		MethodInfo mapMethod;
		lock (CacheLock)
		{
			if (!MapMethodCache.TryGetValue(mapperType, out mapMethod!))
			{
				mapMethod = mapperType.GetMethod(nameof(IIntegrationEventMapper<IDomainEvent>.Map))
					?? throw new InvalidOperationException($"Mapper type '{mapperType.Name}' does not expose a Map method.");
				MapMethodCache[mapperType] = mapMethod;
			}
		}

		var result = mapMethod.Invoke(mapper, [domainEvent]);
		if (result is null)
			return [];

		return result as IEnumerable<IIntegrationEvent>
			?? throw new InvalidOperationException($"Mapper type '{mapperType.Name}' returned an invalid mapping result.");
	}
	#endregion
}
