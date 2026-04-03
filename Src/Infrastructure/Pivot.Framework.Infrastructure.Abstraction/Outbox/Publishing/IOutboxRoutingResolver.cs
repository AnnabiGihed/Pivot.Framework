using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Resolves the broker route for an outbox message at publish time.
///              Implementations are service-specific and may inspect the message kind,
///              event type, or payload metadata to select the appropriate exchange
///              and routing key.
/// </summary>
public interface IOutboxRoutingResolver
{
	/// <summary>
	/// Resolves the route for the specified outbox <paramref name="message"/>.
	/// </summary>
	/// <param name="message">The outbox message to route. Must not be null.</param>
	/// <returns>The resolved exchange/routing-key pair.</returns>
	OutboxRoute Resolve(OutboxMessage message);
}
