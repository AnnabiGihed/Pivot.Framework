using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.DomainEventHandler;

namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Non-generic marker interface for projection handlers.
///              Enables DI container resolution of all projection handlers regardless of
///              the event type they handle.
/// </summary>
public interface IProjectionHandler
{
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Typed contract for projection handlers.
///              Used by <see cref="IProjectionDispatcher"/> to invoke <b>only</b> projection
///              handlers during safe dispatch scenarios — ensuring no side-effect handlers
///              (emails, notifications, commands) are triggered.
/// </summary>
/// <typeparam name="TEvent">The domain event type this handler projects.</typeparam>
public interface IProjectionHandler<in TEvent> : IProjectionHandler
	where TEvent : IDomainEvent
{
	/// <summary>
	/// Projects the domain event into a read model update.
	/// Implementations must be idempotent — safe to call multiple times with the same event.
	/// </summary>
	/// <param name="domainEvent">The domain event to project.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	Task ProjectAsync(TEvent domainEvent, CancellationToken ct);
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Base class for event-driven read model updates (projections).
///
///              Dual dispatch design:
///              <list type="bullet">
///              <item><description>
///              Extends <see cref="DomainEventHandlerBase{TEvent}"/> — MediatR picks it up as
///              <c>INotificationHandler</c>, so it fires during normal outbox → transport → MediatR flow.
///              </description></item>
///              <item><description>
///              Implements <see cref="IProjectionHandler{TEvent}"/> — <see cref="IProjectionDispatcher"/>
///              resolves and invokes it directly for projection-only scenarios (no side effects).
///              </description></item>
///              </list>
///
///              Subclasses override <see cref="ProjectAsync"/> and inject <see cref="IReadModelStore{TReadModel,TId}"/>
///              to persist read model state. The <see cref="IReadModelStore{TReadModel,TId}.UpsertAsync"/>
///              method guarantees idempotency for at-least-once delivery.
/// </summary>
/// <typeparam name="TEvent">The domain event type this handler projects.</typeparam>
public abstract class ProjectionHandler<TEvent> : DomainEventHandlerBase<TEvent>,
	IProjectionHandler<TEvent>
	where TEvent : IDomainEvent
{
	#region Public Methods
	/// <summary>
	/// Sealed entry point from MediatR. Delegates to <see cref="ProjectAsync"/>
	/// and returns <see cref="Result.Success()"/>.
	/// Sealed to enforce the projection contract — subclasses must override
	/// <see cref="ProjectAsync"/> instead.
	/// </summary>
	public sealed override async Task<Result> HandleWithResultAsync(
		TEvent domainEvent, CancellationToken cancellationToken)
	{
		await ProjectAsync(domainEvent, cancellationToken);
		return Result.Success();
	}

	/// <summary>
	/// Implement to update read model state from the domain event.
	/// This method must be idempotent — use <see cref="IReadModelStore{TReadModel,TId}.UpsertAsync"/>
	/// for safe insert-or-update semantics.
	/// </summary>
	/// <param name="domainEvent">The domain event to project.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	public abstract Task ProjectAsync(TEvent domainEvent, CancellationToken ct);
	#endregion
}
