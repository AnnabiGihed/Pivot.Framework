namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Declares the full RabbitMQ topology (exchanges, queues, bindings, DLQs,
///              retry/delay queues) at application startup. Ensures idempotent declaration
///              so services can safely restart without topology conflicts.
/// </summary>
public interface IRabbitMQTopologyManager
{
	/// <summary>
	/// Declares all exchanges, queues, bindings, dead-letter exchanges, and retry delay queues
	/// defined in the <see cref="TopologyOptions"/>.
	/// </summary>
	Task DeclareTopologyAsync(CancellationToken cancellationToken = default);
}
