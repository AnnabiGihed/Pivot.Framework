namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Base class for all domain entities.
///              Provides identity-based equality semantics only.
///              All derived entity identifiers must implement <see cref="IStronglyTypedId{TId}"/>.
///              For auditing support, derive from <see cref="AuditableEntity{TId}"/>.
///              For auditing and soft-delete support, derive from <see cref="FullEntity{TId}"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type for this entity.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
	where TId : IStronglyTypedId<TId>
{
	#region Properties
	/// <summary>
	/// Gets the unique identifier of this entity.
	/// </summary>
	public virtual TId Id { get; protected init; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="Entity{TId}"/> with the specified identity.
	/// </summary>
	/// <param name="id">The strongly-typed identifier for this entity.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
	protected Entity(TId id)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
	}

	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	protected Entity()
	{
		Id = default!;
	}
	#endregion

	#region Equality
	/// <summary>
	/// Determines whether this entity is equal to another object.
	/// Equality is identity-based: two entities are equal if and only if they share the same runtime type
	/// and the same <see cref="Id"/>.
	/// </summary>
	/// <param name="obj">The object to compare against.</param>
	/// <returns><c>true</c> when both entities have the same type and the same identifier; otherwise <c>false</c>.</returns>
	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return obj is Entity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
	}

	/// <summary>
	/// Returns a hash code derived from the entity's runtime type and identifier.
	/// </summary>
	/// <returns>A hash code for this entity.</returns>
	public override int GetHashCode()
	{
		return HashCode.Combine(GetType(), Id);
	}

	/// <summary>
	/// Determines whether this entity is equal to another entity of the same type.
	/// Delegates to <see cref="Equals(object?)"/> for identity-based comparison.
	/// </summary>
	/// <param name="other">The other entity to compare against.</param>
	/// <returns><c>true</c> when both entities have the same type and identifier; otherwise <c>false</c>.</returns>
	public bool Equals(Entity<TId>? other) => Equals((object?)other);

	/// <summary>
	/// Equality operator. Returns <c>true</c> when both operands refer to the same entity.
	/// </summary>
	/// <param name="left">Left operand.</param>
	/// <param name="right">Right operand.</param>
	/// <returns><c>true</c> when equal; otherwise <c>false</c>.</returns>
	public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Inequality operator. Returns <c>true</c> when the operands refer to different entities.
	/// </summary>
	/// <param name="left">Left operand.</param>
	/// <param name="right">Right operand.</param>
	/// <returns><c>true</c> when not equal; otherwise <c>false</c>.</returns>
	public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
	{
		return !Equals(left, right);
	}
	#endregion
}
