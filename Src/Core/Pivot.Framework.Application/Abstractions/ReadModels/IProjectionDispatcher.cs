using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Abstractions.ReadModels;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Dispatches domain events to <b>only</b> <see cref="IProjectionHandler{TEvent}"/>
///              implementations.
///
///              Unlike <c>IDomainEventDispatcher</c> (which publishes to ALL MediatR handlers
///              including side-effect handlers like email senders and notification services),
///              this dispatcher targets projection handlers exclusively.
///
///              Use cases:
///              <list type="bullet">
///              <item><description>
///              Safe projection-only invocation where side effects must not fire.
///              </description></item>
///              <item><description>
///              Selective replay of events to rebuild specific read models.
///              </description></item>
///              </list>
/// </summary>
public interface IProjectionDispatcher
{
	/// <summary>
	/// Dispatches the given <paramref name="domainEvent"/> to all registered
	/// <see cref="IProjectionHandler{TEvent}"/> instances for the event's type.
	/// No side-effect handlers are invoked.
	/// </summary>
	/// <param name="domainEvent">The domain event to dispatch. Must not be null.</param>
	/// <param name="ct">Token to observe for cooperative cancellation.</param>
	Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
