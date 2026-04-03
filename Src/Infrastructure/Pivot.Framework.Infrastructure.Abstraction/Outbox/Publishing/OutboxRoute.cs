namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Represents the resolved broker route for an outbox message.
///              Allows publishers to target different exchanges and routing keys
///              per message while keeping transport-specific routing logic outside
///              the publisher implementation.
/// </summary>
/// <param name="Exchange">The exchange to publish to.</param>
/// <param name="RoutingKey">The routing key to use when publishing.</param>
public sealed record OutboxRoute(string Exchange, string RoutingKey);
