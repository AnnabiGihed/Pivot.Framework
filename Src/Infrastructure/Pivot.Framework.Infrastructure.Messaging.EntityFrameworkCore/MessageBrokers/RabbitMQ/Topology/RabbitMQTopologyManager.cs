using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Topology;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : RabbitMQ implementation of <see cref="IRabbitMQTopologyManager"/>.
///              Declares the full broker topology at startup: topic exchanges, quorum queues,
///              dead-letter exchanges/queues, and retry delay queues. All declarations are
///              idempotent — safe to call on every service restart.
///
///              Implements MDM spec requirements:
///              - Section 22: Topic exchanges, quorum queues, per-queue DLX/DLQ
///              - Invariant #19: Every production queue must define retry, DLQ, and poison-message procedures
/// </summary>
public sealed class RabbitMQTopologyManager : IRabbitMQTopologyManager, IAsyncDisposable
{
	private readonly RabbitMQSettings _settings;
	private readonly TopologyOptions _topologyOptions;
	private readonly ILogger<RabbitMQTopologyManager> _logger;
	private IConnection? _connection;
	private IChannel? _channel;

	public RabbitMQTopologyManager(
		IOptions<RabbitMQSettings> settings,
		IOptions<TopologyOptions> topologyOptions,
		ILogger<RabbitMQTopologyManager> logger)
	{
		_settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
		_topologyOptions = topologyOptions.Value ?? throw new ArgumentNullException(nameof(topologyOptions));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task DeclareTopologyAsync(CancellationToken cancellationToken = default)
	{
		var factory = new ConnectionFactory
		{
			HostName = _settings.HostName,
			UserName = _settings.UserName,
			Password = _settings.Password,
			VirtualHost = _settings.VirtualHost,
			Port = _settings.Port,
			ClientProvidedName = $"{_settings.ClientProvidedName}-topology"
		};

		_connection = await factory.CreateConnectionAsync(cancellationToken);
		_channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

		foreach (var binding in _topologyOptions.Bindings)
		{
			await DeclareBindingAsync(binding, cancellationToken);
		}

		_logger.LogInformation("RabbitMQ topology declared: {BindingCount} bindings", _topologyOptions.Bindings.Count);
	}

	private async Task DeclareBindingAsync(ExchangeBinding binding, CancellationToken ct)
	{
		if (_channel is null) return;

		// 1. Declare the main exchange (topic)
		await _channel.ExchangeDeclareAsync(
			binding.Exchange,
			binding.ExchangeType,
			durable: true,
			autoDelete: false,
			arguments: null,
			cancellationToken: ct);

		_logger.LogDebug("Declared exchange: {Exchange} ({Type})", binding.Exchange, binding.ExchangeType);

		// 2. Build queue arguments
		var queueArgs = new Dictionary<string, object?>();

		if (binding.UseQuorumQueue)
		{
			queueArgs["x-queue-type"] = "quorum";
		}

		// 3. Declare DLX and DLQ if enabled
		if (binding.EnableDeadLetterQueue)
		{
			var dlxName = $"{binding.Exchange}.dlx";
			var dlqName = $"{binding.Queue}.dlq";

			// Declare dead-letter exchange
			await _channel.ExchangeDeclareAsync(
				dlxName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null, cancellationToken: ct);

			// Declare dead-letter queue
			var dlqArgs = new Dictionary<string, object?>();
			if (binding.UseQuorumQueue)
				dlqArgs["x-queue-type"] = "quorum";

			await _channel.QueueDeclareAsync(
				dlqName, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs, cancellationToken: ct);

			await _channel.QueueBindAsync(
				dlqName, dlxName, binding.Queue, arguments: null, cancellationToken: ct);

			// Set DLX on the main queue
			queueArgs["x-dead-letter-exchange"] = dlxName;
			queueArgs["x-dead-letter-routing-key"] = binding.Queue;

			_logger.LogDebug("Declared DLX: {DlxName}, DLQ: {DlqName}", dlxName, dlqName);
		}

		// 4. Declare retry delay queue if retry is enabled
		if (binding.MaxRetryCount > 0 && binding.RetryDelayMs > 0)
		{
			var retryQueueName = $"{binding.Queue}.retry";
			var retryArgs = new Dictionary<string, object?>
			{
				["x-dead-letter-exchange"] = binding.Exchange,
				["x-dead-letter-routing-key"] = binding.RoutingKey,
				["x-message-ttl"] = binding.RetryDelayMs
			};

			if (binding.UseQuorumQueue)
				retryArgs["x-queue-type"] = "quorum";

			await _channel.QueueDeclareAsync(
				retryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs, cancellationToken: ct);

			_logger.LogDebug("Declared retry queue: {RetryQueue} (delay: {DelayMs}ms)", retryQueueName, binding.RetryDelayMs);
		}

		// 5. Declare the main queue
		await _channel.QueueDeclareAsync(
			binding.Queue, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: ct);

		// 6. Bind queue to exchange with routing key
		await _channel.QueueBindAsync(
			binding.Queue, binding.Exchange, binding.RoutingKey, arguments: null, cancellationToken: ct);

		_logger.LogDebug("Declared queue: {Queue}, bound to {Exchange} with key {RoutingKey}",
			binding.Queue, binding.Exchange, binding.RoutingKey);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		try { if (_channel is not null) await _channel.CloseAsync(); } catch { }
		try { if (_connection is not null) await _connection.CloseAsync(); } catch { }
		_channel?.Dispose();
		_connection?.Dispose();
	}
}
