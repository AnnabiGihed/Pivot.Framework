using Templates.Core.Domain.Primitives;

namespace Templates.Core.Domain.DomainEvents;

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;
