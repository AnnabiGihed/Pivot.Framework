namespace Templates.Core.Domain.Primitives;

public interface IAggregateRoot
{
	IReadOnlyCollection<IDomainEvent> GetDomainEvents();

	void ClearDomainEvents();

}