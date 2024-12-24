using MediatR;
using Templates.Core.Domain.Primitives;

namespace Templates.Core.Application.Abstrations.Messaging.Events;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : IDomainEvent
{
}