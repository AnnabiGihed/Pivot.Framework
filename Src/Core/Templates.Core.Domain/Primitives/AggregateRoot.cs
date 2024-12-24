using CSharpFunctionalExtensions;

namespace Templates.Core.Domain.Primitives;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : IComparable<TId>
{
	private readonly List<IDomainEvent> _domainEvents = new();

	protected AggregateRoot(TId id)
		: base(id)
	{
	}

	protected AggregateRoot()
	{
	}

	public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

	public void ClearDomainEvents() => _domainEvents.Clear();

	protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
		_domainEvents.Add(domainEvent);
}
