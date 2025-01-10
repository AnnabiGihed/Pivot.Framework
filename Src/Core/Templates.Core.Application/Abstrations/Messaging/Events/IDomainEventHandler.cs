using MediatR;
using Templates.Core.Domain.Shared;
using Templates.Core.Domain.Primitives;

namespace Templates.Core.Application.Abstrations.Messaging.Events;

/// <summary>
/// Interface for domain event handlers that return a result.
/// </summary>
/// <typeparam name="TEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : IDomainEvent
{
	Task<Result> HandleWithResultAsync(TEvent domainEvent, CancellationToken cancellationToken);
}