using MediatR;

namespace Templates.Core.Domain.Primitives;

public interface IDomainEvent : INotification
{
	public Guid Id { get; init; }
	public DateTime OccurredOnUtc { get; init; }
}
