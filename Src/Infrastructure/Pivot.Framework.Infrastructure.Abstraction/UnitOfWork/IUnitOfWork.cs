using Pivot.Framework.Domain.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.UnitOfWork;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : DI-scoped variant of <see cref="IUnitOfWork"/>.
///               The generic parameter <typeparamref name="TContext"/> acts as a DI scoping key
///               so that multiple persistence contexts can coexist in the same process without ambiguity.
///               For example, a process hosting both CurviaDbContext and an AuditDbContext
///               can register and inject IUnitOfWork&lt;CurviaDbContext&gt; and
///               IUnitOfWork&lt;AuditDbContext&gt; independently.
///
///               WHY this lives in Infrastructure.Abstraction and not Domain:
///               Infrastructure.Abstraction is the correct home for persistence-aware
///               abstractions that should not live in Domain.
///
///               Consumers (command handlers) depend on this interface.
///               The concrete implementation is UnitOfWork&lt;TContext&gt; in
///               Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.
/// </summary>
/// <typeparam name="TContext">
/// The persistence context type. Used exclusively as a DI discriminator — it is never
/// used as an entity ID or a domain concept.
/// </typeparam>
public interface IUnitOfWork<TContext> : IUnitOfWork
	where TContext : class, IPersistenceContext
{
	// No additional members.
	// SaveChangesAsync is inherited from IUnitOfWork.
	// The generic parameter exists solely to enable distinct DI registrations per persistence context.
}
