namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Extends <see cref="AuditableEntity{TId}"/> with soft-delete support
///              through <see cref="ISoftDeletableEntity"/>.
///              This is the equivalent of the original <see cref="Entity{TId}"/> before the
///              Interface Segregation refactoring. Entities that need both auditing and
///              soft-delete behaviour should derive from this class.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type for this entity.</typeparam>
public abstract class FullEntity<TId> : AuditableEntity<TId>, ISoftDeletableEntity
	where TId : IStronglyTypedId<TId>
{
	#region Properties
	/// <summary>
	/// Gets a value indicating whether this entity has been soft-deleted.
	/// </summary>
	public virtual bool IsDeleted { get; protected set; }

	/// <summary>
	/// Gets the actor who soft-deleted this entity, if applicable.
	/// </summary>
	public virtual string? DeletedBy { get; protected set; }

	/// <summary>
	/// Gets the UTC timestamp at which this entity was soft-deleted, if applicable.
	/// </summary>
	public virtual DateTime? DeletedOnUtc { get; protected set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="FullEntity{TId}"/> with the specified identity.
	/// </summary>
	/// <param name="id">The strongly-typed identifier for this entity.</param>
	protected FullEntity(TId id) : base(id)
	{
	}

	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	protected FullEntity() : base()
	{
	}
	#endregion

	#region Soft Delete
	/// <summary>
	/// Explicit interface implementation — marks this entity as soft-deleted.
	/// Called by the repository or domain method via the interface contract.
	/// </summary>
	/// <param name="deletedOnUtc">UTC timestamp of deletion.</param>
	/// <param name="deletedBy">Actor who performed the deletion.</param>
	void ISoftDeletableEntity.MarkDeleted(DateTime deletedOnUtc, string deletedBy)
	{
		SoftDelete(deletedOnUtc, deletedBy);
	}

	/// <summary>
	/// Explicit interface implementation — restores a previously soft-deleted entity.
	/// Called by the repository or domain method via the interface contract.
	/// </summary>
	/// <param name="restoredOnUtc">UTC timestamp of restoration.</param>
	/// <param name="restoredBy">Actor who performed the restoration.</param>
	void ISoftDeletableEntity.MarkRestored(DateTime restoredOnUtc, string restoredBy)
	{
		Restore(restoredOnUtc, restoredBy);
	}

	/// <summary>
	/// Soft-deletes this entity. Idempotent — calling twice has no effect.
	/// Also updates audit metadata to reflect the deletion.
	/// </summary>
	/// <param name="deletedOnUtc">UTC timestamp of deletion.</param>
	/// <param name="deletedBy">Actor who performed the deletion.</param>
	protected void SoftDelete(DateTime deletedOnUtc, string deletedBy)
	{
		if (IsDeleted)
		{
			return;
		}

		if (deletedOnUtc == DateTime.MinValue || deletedOnUtc == DateTime.MaxValue)
			throw new ArgumentException("Date must be a valid value.", nameof(deletedOnUtc));
		if (deletedOnUtc.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Date must be expressed in UTC.", nameof(deletedOnUtc));

		ArgumentNullException.ThrowIfNull(deletedBy, nameof(deletedBy));
		if (string.IsNullOrWhiteSpace(deletedBy))
			throw new ArgumentException("Actor must not be empty or whitespace.", nameof(deletedBy));

		IsDeleted = true;
		DeletedOnUtc = deletedOnUtc;
		DeletedBy = deletedBy;

		if (Audit is not null)
		{
			Audit = Audit.Modify(deletedOnUtc, deletedBy);
		}
	}

	/// <summary>
	/// Restores a previously soft-deleted entity. Idempotent — calling twice has no effect.
	/// Also updates audit metadata to reflect the restoration.
	/// </summary>
	/// <param name="restoredOnUtc">UTC timestamp of restoration.</param>
	/// <param name="restoredBy">Actor who performed the restoration.</param>
	protected void Restore(DateTime restoredOnUtc, string restoredBy)
	{
		if (!IsDeleted)
		{
			return;
		}

		if (restoredOnUtc == DateTime.MinValue || restoredOnUtc == DateTime.MaxValue)
			throw new ArgumentException("Date must be a valid value.", nameof(restoredOnUtc));
		if (restoredOnUtc.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Date must be expressed in UTC.", nameof(restoredOnUtc));

		ArgumentNullException.ThrowIfNull(restoredBy, nameof(restoredBy));
		if (string.IsNullOrWhiteSpace(restoredBy))
			throw new ArgumentException("Actor must not be empty or whitespace.", nameof(restoredBy));

		IsDeleted = false;
		DeletedOnUtc = null;
		DeletedBy = null;

		if (Audit is not null)
		{
			Audit = Audit.Modify(restoredOnUtc, restoredBy);
		}
	}
	#endregion
}
