namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Contract for read models (projections) in a CQRS architecture.
///              Read models are denormalised, query-optimised representations of domain state.
///              They live in the Application layer — not the Domain layer — because they serve
///              query needs rather than enforcing business invariants.
///
///              <typeparamref name="TId"/> is unconstrained: read models may use
///              <see cref="Guid"/>, <see cref="int"/>, <see cref="string"/>, or any other
///              key type appropriate for the read store (SQL, MongoDB, etc.).
/// </summary>
/// <typeparam name="TId">The identifier type of the read model.</typeparam>
public interface IReadModel<TId>
{
	/// <summary>
	/// Gets the unique identifier of this read model instance.
	/// </summary>
	TId Id { get; }
}
