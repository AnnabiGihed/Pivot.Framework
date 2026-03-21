using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Application.Abstractions.Messaging.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Abstraction for dispatching domain events to in-process handlers.
///              The Application layer defines this contract; the Infrastructure layer
///              provides the implementation (e.g., MediatR-based, or any other dispatcher).
///              This follows the Dependency Inversion Principle — the domain and application
///              layers never reference the dispatching framework directly.
/// </summary>
public interface IDomainEventDispatcher
{
	/// <summary>
	/// Dispatches a domain event to all registered handlers.
	/// The implementation is responsible for wrapping the event in the appropriate
	/// notification type (e.g., <see cref="DomainEventNotification{TDomainEvent}"/>)
	/// before publishing.
	/// </summary>
	/// <param name="domainEvent">The domain event to dispatch.</param>
	/// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
	Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
