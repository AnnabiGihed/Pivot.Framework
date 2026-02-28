using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Domain.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 02-2026 — Removed incorrect IStronglyTypedId generic constraint.
///               The DI-scoping discriminator belongs in Infrastructure.Abstraction
///               (IUnitOfWork&lt;TContext&gt;) where DbContext is a valid constraint.
///               Domain only knows: "something that can atomically save changes."
/// Purpose     : Coordinates a single atomic database operation: auditing entity changes,
///               flushing pending domain events to the outbox, and committing via
///               EF Core SaveChangesAsync.
///               Implemented by IUnitOfWork&lt;TContext&gt; in Infrastructure.Abstraction,
///               which adds the DbContext-scoped DI discriminator.
/// </summary>
public interface IUnitOfWork
{
	/// <summary>
	/// Atomically persists all pending entity changes.
	/// Internally: stamps audit fields on modified entities, serialises raised domain events to the outbox,
	/// then calls EF Core SaveChangesAsync within the ambient transaction.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>
	/// A <see cref="Result"/> that is successful when the commit succeeds, or a failure carrying
	/// the underlying database error (concurrency conflict, constraint violation, etc.).
	/// </returns>
	Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}