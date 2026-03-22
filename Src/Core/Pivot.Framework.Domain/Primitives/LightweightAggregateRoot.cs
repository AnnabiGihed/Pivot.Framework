namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Lightweight aggregate root that extends <see cref="Entity{TId}"/> only.
///              Use this base class for aggregates that do not require auditing or soft-delete
///              behaviour — only identity and domain event support are provided.
///              For auditing without soft-delete, use <see cref="AuditableAggregateRoot{TId}"/>.
///              For full auditing and soft-delete, use <see cref="AggregateRoot{TId}"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the aggregate root.</typeparam>
public abstract class LightweightAggregateRoot<TId> : Entity<TId>, IAggregateRoot
	where TId : IStronglyTypedId<TId>
{
	#region Fields
	/// <summary>
	/// Internal store of domain events raised during this aggregate's lifecycle.
	/// </summary>
	private readonly List<IDomainEvent> _domainEvents = new();
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="LightweightAggregateRoot{TId}"/> with the specified identity.
	/// </summary>
	/// <param name="id">The strongly-typed identifier for this aggregate root.</param>
	protected LightweightAggregateRoot(TId id) : base(id)
	{
	}

	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	protected LightweightAggregateRoot()
	{
	}
	#endregion

	#region Domain Events
	/// <summary>
	/// Returns all domain events raised by this aggregate since the last time they were cleared.
	/// </summary>
	/// <returns>A read-only collection of pending domain events.</returns>
	public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
	{
		return _domainEvents.AsReadOnly();
	}

	/// <summary>
	/// Clears all stored domain events from this aggregate.
	/// </summary>
	void IAggregateRoot.ClearDomainEvents()
	{
		_domainEvents.Clear();
	}

	/// <summary>
	/// Registers a new domain event to be dispatched after the current unit of work commits.
	/// </summary>
	/// <param name="domainEvent">The domain event to raise.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="domainEvent"/> is null.</exception>
	protected void RaiseDomainEvent(IDomainEvent domainEvent)
	{
		ArgumentNullException.ThrowIfNull(domainEvent);
		_domainEvents.Add(domainEvent);
	}
	#endregion
}
