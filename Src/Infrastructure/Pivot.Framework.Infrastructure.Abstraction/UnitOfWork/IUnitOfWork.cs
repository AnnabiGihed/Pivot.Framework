using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Repositories;

namespace Pivot.Framework.Infrastructure.Abstraction.UnitOfWork;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : DI-scoped variant of <see cref="IUnitOfWork"/>.
///               The generic parameter <typeparamref name="TContext"/> acts as a DI scoping key
///               so that multiple DbContexts can coexist in the same process without ambiguity.
///               For example, a process hosting both CurviaDbContext and an AuditDbContext
///               can register and inject IUnitOfWork&lt;CurviaDbContext&gt; and
///               IUnitOfWork&lt;AuditDbContext&gt; independently.
///
///               WHY this lives in Infrastructure.Abstraction and not Domain:
///               The constraint "where TContext : DbContext" introduces a dependency on
///               Microsoft.EntityFrameworkCore, which Domain must never reference.
///               Infrastructure.Abstraction already carries that dependency (IOutboxRepository,
///               ITransactionManager), so this is the correct home.
///
///               Consumers (command handlers) depend on this interface.
///               The concrete implementation is UnitOfWork&lt;TContext&gt; in
///               Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.
/// </summary>
/// <typeparam name="TContext">
/// The EF Core DbContext type. Used exclusively as a DI discriminator — it is never
/// used as an entity ID or a domain concept.
/// </typeparam>
public interface IUnitOfWork<TContext> : IUnitOfWork
	where TContext : DbContext
{
	// No additional members.
	// SaveChangesAsync is inherited from IUnitOfWork.
	// The generic parameter exists solely to enable distinct DI registrations per DbContext.
}