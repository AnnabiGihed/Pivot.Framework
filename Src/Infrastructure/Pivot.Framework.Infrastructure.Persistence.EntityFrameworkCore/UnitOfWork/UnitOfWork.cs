using Microsoft.AspNetCore.Http;
using Pivot.Framework.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Infrastructure.Abstraction.UnitOfWork;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.UnitOfWork;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 02-2026 — Fixed generic constraint from "TId : IStronglyTypedId&lt;TId&gt;" to
///               "TContext : DbContext" so that consumers can correctly inject
///               IUnitOfWork&lt;CurviaDbContext&gt; (or any other DbContext) without requiring
///               the DbContext to implement IStronglyTypedId, which was semantically wrong.
/// Purpose     : Transaction-agnostic Unit of Work.
///               Persists business data and outbox messages in the ambient transaction
///               (transaction is owned by TransactionMiddleware, not here).
///               Concrete subclasses supply the typed DbContext via the constructor;
///               this base class handles all auditing, outbox flushing, and commit logic.
/// </summary>
/// <typeparam name="TContext">
/// The EF Core DbContext type scoped to this unit of work.
/// Used as a DI discriminator and to resolve the concrete DbContext from DI.
/// </typeparam>
public abstract class UnitOfWork<TContext> : IUnitOfWork<TContext>
	where TContext : DbContext
{
	protected readonly DbContext _dbContext;
	protected readonly IHttpContextAccessor _httpContextAccessor;
	protected readonly IDomainEventPublisher _domainEventPublisher;

	protected UnitOfWork(TContext dbContext, IHttpContextAccessor httpContextAccessor, IDomainEventPublisher domainEventPublisher)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		_domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
	}

	public virtual async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			UpdateAuditableEntities();

			var persistEventsResult = await PersistDomainEventsToOutboxAsync(cancellationToken);
			if (persistEventsResult.IsFailure)
				return persistEventsResult;

			await _dbContext.SaveChangesAsync(cancellationToken);

			ClearDomainEvents();

			return Result.Success();
		}
		catch (DbUpdateConcurrencyException ex)
		{
			return Result.Failure(new Error("DbUpdateConcurrencyError", ex.Message));
		}
		catch (DbUpdateException ex)
		{
			return Result.Failure(new Error("DatabaseError", ex.Message));
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("UnexpectedError", ex.Message));
		}
	}

	protected virtual void UpdateAuditableEntities()
	{
		var now = DateTime.UtcNow;

		var identity = _httpContextAccessor.HttpContext?.User?.Identity;
		var actor = (identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(identity.Name))
			? identity.Name!
			: "System";

		foreach (var entry in _dbContext.ChangeTracker.Entries<IAuditableEntity>())
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.SetAudit(AuditInfo.Create(now, actor));
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.Audit.Modify(now, actor);
			}
		}
	}

	protected virtual async Task<Result> PersistDomainEventsToOutboxAsync(CancellationToken cancellationToken)
	{
		try
		{
			var aggregates = _dbContext.ChangeTracker
				.Entries<IAggregateRoot>()
				.Select(e => e.Entity)
				.Where(a => a.GetDomainEvents().Any())
				.ToList();

			foreach (var aggregate in aggregates)
			{
				foreach (var domainEvent in aggregate.GetDomainEvents())
				{
					var result = await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
					if (result.IsFailure)
						return result;
				}
			}

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("DomainEventOutboxError", ex.Message));
		}
	}

	protected virtual void ClearDomainEvents()
	{
		foreach (var entry in _dbContext.ChangeTracker.Entries<IAggregateRoot>())
			entry.Entity.ClearDomainEvents();
	}
}