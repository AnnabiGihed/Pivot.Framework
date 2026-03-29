namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Defines a RabbitMQ exchange binding with topic routing, dead-letter exchange,
///              retry delay, and quorum queue configuration. Used by <see cref="IRabbitMQTopologyManager"/>
///              to declare the full broker topology at startup.
/// </summary>
public sealed class ExchangeBinding
{
	/// <summary>Exchange name (e.g., "mdm.mastering.events").</summary>
	public required string Exchange { get; init; }

	/// <summary>Exchange type. Defaults to "topic".</summary>
	public string ExchangeType { get; init; } = "topic";

	/// <summary>Queue name bound to this exchange.</summary>
	public required string Queue { get; init; }

	/// <summary>Routing key pattern (e.g., "mastering.completed").</summary>
	public required string RoutingKey { get; init; }

	/// <summary>Consumer name for inbox deduplication.</summary>
	public string? ConsumerName { get; init; }

	/// <summary>Whether to configure a dead-letter exchange and DLQ for this queue.</summary>
	public bool EnableDeadLetterQueue { get; init; } = true;

	/// <summary>Number of retry attempts before dead-lettering. Defaults to 3.</summary>
	public int MaxRetryCount { get; init; } = 3;

	/// <summary>Delay in milliseconds between retries (for delay queue pattern). Defaults to 5000.</summary>
	public int RetryDelayMs { get; init; } = 5000;

	/// <summary>Whether to use quorum queues (required for production per MDM spec). Defaults to true.</summary>
	public bool UseQuorumQueue { get; init; } = true;
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration options for the full RabbitMQ topology of a service.
///              Defines all exchanges, queues, bindings, and retry/DLQ policies to be
///              declared at application startup.
/// </summary>
public sealed class TopologyOptions
{
	/// <summary>
	/// The list of exchange bindings to declare at startup.
	/// </summary>
	public List<ExchangeBinding> Bindings { get; } = new();

	/// <summary>
	/// Adds an exchange binding to the topology.
	/// </summary>
	public TopologyOptions Bind(ExchangeBinding binding)
	{
		Bindings.Add(binding);
		return this;
	}
}
