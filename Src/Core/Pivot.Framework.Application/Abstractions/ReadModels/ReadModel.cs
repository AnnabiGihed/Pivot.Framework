namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Convenience base class for read models (projections) in a CQRS architecture.
///              Provides identity storage and a parameterless constructor for ORM hydration
///              (EF Core, MongoDB, etc.).
///
///              <typeparamref name="TId"/> is unconstrained: read models may use
///              <see cref="Guid"/>, <see cref="int"/>, <see cref="string"/>, or any other
///              key type appropriate for the read store.
/// </summary>
/// <typeparam name="TId">The identifier type of the read model.</typeparam>
public abstract class ReadModel<TId> : IReadModel<TId>
{
	#region Properties
	/// <summary>
	/// Gets or sets the unique identifier of this read model.
	/// Marked <c>virtual</c> so that ORM proxies can override it if needed.
	/// </summary>
	public virtual TId Id { get; protected set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="ReadModel{TId}"/> with the specified identifier.
	/// </summary>
	/// <param name="id">The identifier. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
	protected ReadModel(TId id)
	{
		ArgumentNullException.ThrowIfNull(id);
		Id = id;
	}

	/// <summary>
	/// Parameterless constructor for ORM materialisation (EF Core, MongoDB, Dapper, etc.).
	/// </summary>
	protected ReadModel()
	{
		Id = default!;
	}
	#endregion
}
