namespace Pivot.Framework.Domain.Primitives;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Extends <see cref="Entity{TId}"/> with audit metadata via <see cref="IAuditableEntity"/>.
///              Entities that need creation/modification tracking but not soft-delete should derive from this class.
///              For full auditing plus soft-delete support, derive from <see cref="FullEntity{TId}"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type for this entity.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
	where TId : IStronglyTypedId<TId>
{
	#region Properties
	/// <summary>
	/// Gets the audit metadata (created/modified timestamps and actors) for this entity.
	/// Initialised by the factory on creation; updated by <see cref="Touch"/>.
	/// </summary>
	public virtual AuditInfo? Audit { get; protected set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="AuditableEntity{TId}"/> with the specified identity.
	/// </summary>
	/// <param name="id">The strongly-typed identifier for this entity.</param>
	protected AuditableEntity(TId id) : base(id)
	{
	}

	/// <summary>
	/// Parameterless constructor reserved for EF Core materialisation.
	/// </summary>
	protected AuditableEntity() : base()
	{
	}
	#endregion

	#region Audit
	/// <summary>
	/// Explicit interface implementation that allows the persistence pipeline to initialise
	/// audit metadata without exposing the setter publicly on the domain class.
	/// </summary>
	/// <param name="audit">The <see cref="AuditInfo"/> value object to assign.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="audit"/> is null.</exception>
	void IAuditableEntity.SetAudit(AuditInfo audit)
	{
		ArgumentNullException.ThrowIfNull(audit);
		Audit = audit;
	}

	/// <summary>
	/// Initialises the audit record on entity creation.
	/// Must be called once by the factory method before the entity is returned.
	/// </summary>
	/// <param name="createdOnUtc">UTC timestamp of creation.</param>
	/// <param name="createdBy">Actor who created this entity.</param>
	protected void InitializeAudit(DateTime createdOnUtc, string createdBy)
	{
		Audit = AuditInfo.Create(createdOnUtc, createdBy);
	}

	/// <summary>
	/// Updates the audit record to reflect a modification.
	/// Throws if audit has not been initialised.
	/// </summary>
	/// <param name="modifiedOnUtc">UTC timestamp of modification.</param>
	/// <param name="modifiedBy">Actor who performed the modification.</param>
	protected void Touch(DateTime modifiedOnUtc, string modifiedBy)
	{
		EnsureAuditInitialized();
		Audit = Audit!.Modify(modifiedOnUtc, modifiedBy);
	}

	/// <summary>
	/// Protected overload of <see cref="IAuditableEntity.SetAudit"/> available to derived classes
	/// for explicit audit assignment (e.g., during reconstitution).
	/// </summary>
	/// <param name="audit">The <see cref="AuditInfo"/> value to assign.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="audit"/> is null.</exception>
	protected void SetAudit(AuditInfo audit)
	{
		ArgumentNullException.ThrowIfNull(audit);
		Audit = audit;
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Throws <see cref="InvalidOperationException"/> when <see cref="Audit"/> has not been
	/// initialised. Guards against calling <see cref="Touch"/> before creation.
	/// </summary>
	private void EnsureAuditInitialized()
	{
		if (Audit is null)
		{
			throw new InvalidOperationException("AuditInfo must be initialised before performing audit updates. " + "Call InitializeAudit() during entity creation or ensure the persistence layer materialises Audit.");
		}
	}
	#endregion
}
