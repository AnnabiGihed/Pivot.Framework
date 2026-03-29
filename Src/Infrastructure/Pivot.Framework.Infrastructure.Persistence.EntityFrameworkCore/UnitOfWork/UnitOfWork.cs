using Pivot.Framework.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
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
	where TContext : DbContext, IPersistenceContext
{
	#region Fields
	/// <summary>
	/// The EF Core database context used for persistence operations.
	/// </summary>
	protected readonly DbContext _dbContext;

	/// <summary>
	/// Provider for resolving the current authenticated user for audit stamping.
	/// </summary>
	protected readonly ICurrentUserProvider _currentUserProvider;

	/// <summary>
	/// Publisher that serializes domain events into the outbox for reliable delivery.
	/// </summary>
	protected readonly IDomainEventPublisher _domainEventPublisher;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="UnitOfWork{TContext}"/> with the provided dependencies.
	/// </summary>
	/// <param name="dbContext">The EF Core database context. Must not be null.</param>
	/// <param name="currentUserProvider">The current user provider for audit stamping. Must not be null.</param>
	/// <param name="domainEventPublisher">The domain event publisher for outbox persistence. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when any of the parameters is null.
	/// </exception>
	protected UnitOfWork(TContext dbContext, ICurrentUserProvider currentUserProvider, IDomainEventPublisher domainEventPublisher)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
		_domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Persists all tracked changes to the database. Before saving, auditable entities
	/// are stamped with creation/modification metadata and domain events are serialized
	/// into the outbox for reliable delivery.
	/// </summary>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or the first encountered failure.</returns>
	public virtual async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			UpdateAuditableEntities();

			var aggregatesWithEvents = _dbContext.ChangeTracker
				.Entries<IAggregateRoot>()
				.Where(e => e.Entity.GetDomainEvents().Any())
				.Select(e => e.Entity)
				.ToList();

			var persistEventsResult = await PersistDomainEventsToOutboxAsync(aggregatesWithEvents, cancellationToken);
			if (persistEventsResult.IsFailure)
				return persistEventsResult;

			await _dbContext.SaveChangesAsync(cancellationToken);

			foreach (var aggregate in aggregatesWithEvents)
				aggregate.ClearDomainEvents();

			return Result.Success();
		}
		catch (OperationCanceledException)
		{
			throw;
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
	#endregion

	#region Protected Methods
	/// <summary>
	/// Stamps auditable entities with creation or modification metadata based on their
	/// current change tracker state.
	/// </summary>
	protected virtual void UpdateAuditableEntities()
	{
		var now = DateTime.UtcNow;
		var actor = _currentUserProvider.GetCurrentUser();

		foreach (var entry in _dbContext.ChangeTracker.Entries<IAuditableEntity>())
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.SetAudit(AuditInfo.Create(now, actor));
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.SetAudit(entry.Entity.Audit.Modify(now, actor));
			}
		}
	}

	/// <summary>
	/// Iterates over aggregates that have pending domain events and persists each event
	/// to the outbox via the <see cref="IDomainEventPublisher"/>.
	/// </summary>
	/// <param name="aggregates">The list of aggregates with pending domain events.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	/// <returns>A <see cref="Result"/> indicating success or the first encountered failure.</returns>
	protected virtual async Task<Result> PersistDomainEventsToOutboxAsync(List<IAggregateRoot> aggregates, CancellationToken cancellationToken)
	{
		try
		{
			foreach (var aggregate in aggregates)
			{
				foreach (var domainEvent in aggregate.GetDomainEvents())
				{
					// Use aggregate-aware overload to capture AggregateType/Id/Version in event envelope
					var result = await _domainEventPublisher.PublishAsync(domainEvent, aggregate, cancellationToken);
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
	#endregion
}
