using Polly.Retry;
using RabbitMQ.Client;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Temlates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageCompressor;
using Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageEncryptor;
using Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageSerializer;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessagePublisher;

public class RabbitMQPublisher : IMessagePublisher
{
	#region Properties
	protected readonly RabbitMQSettings _settings;
	protected readonly IMessageEncryptor _encryptor;
	protected readonly AsyncRetryPolicy _retryPolicy;
	protected readonly IMessageCompressor _compressor;
	protected readonly IMessageSerializer _serializer;
	protected readonly ILogger<RabbitMQPublisher> _logger;
	protected readonly AsyncCircuitBreakerPolicy _circuitBreaker;
	protected readonly ConcurrentDictionary<string, bool> _declaredExchanges = new();
	protected readonly ConcurrentDictionary<string, bool> _declaredQueues = new();
	#endregion

	#region Constructor
	public RabbitMQPublisher(IOptions<RabbitMQSettings> options, ILogger<RabbitMQPublisher> logger,	IMessageSerializer serializer,
		IMessageCompressor compressor, IMessageEncryptor encryptor, AsyncRetryPolicy retryPolicy, AsyncCircuitBreakerPolicy circuitBreaker)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_settings = options.Value ?? throw new ArgumentNullException(nameof(options));
		_encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
		_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		_compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
		_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
	}
	#endregion

	public async Task PublishAsync<T>(T message)
	{
		if (message == null) throw new ArgumentNullException(nameof(message));

		await _circuitBreaker.ExecuteAsync(async () =>
		{
			await _retryPolicy.ExecuteAsync(async () =>
			{
				await EnsureExchangeAndQueueAsync();

				using var connection = await CreateConnectionAsync();
				using var channel = await connection.CreateChannelAsync();

				var serializedMessage = _serializer.Serialize(message);
				var compressedMessage = _compressor.Compress(serializedMessage);
				var encryptedMessage = _encryptor.Encrypt(compressedMessage);

				var properties = new BasicProperties
				{
					ContentType = "application/octet-stream",
					DeliveryMode = DeliveryModes.Persistent,
					Headers = new Dictionary<string, object>
					{
						{ "CorrelationId", Guid.NewGuid().ToString() },
						{ "Timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
					}
				};

				await channel.BasicPublishAsync(
					_settings.Exchange,
					_settings.RoutingKey,
					mandatory: true,
					properties,
					encryptedMessage);

				_logger.LogInformation($"Message published to exchange '{_settings.Exchange}' with routing key '{_settings.RoutingKey}'");
			});
		});
	}

	protected async Task EnsureExchangeAndQueueAsync()
	{
		using var connection = await CreateConnectionAsync();
		using var channel = await connection.CreateChannelAsync();

		if (!_declaredExchanges.ContainsKey(_settings.Exchange))
		{
			await channel.ExchangeDeclareAsync(_settings.Exchange, ExchangeType.Direct, durable: true);
			_declaredExchanges.TryAdd(_settings.Exchange, true);
			_logger.LogInformation($"Exchange '{_settings.Exchange}' declared.");
		}

		if (!_declaredQueues.ContainsKey(_settings.Queue))
		{
			await channel.QueueDeclareAsync(_settings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
			await channel.QueueBindAsync(_settings.Queue, _settings.Exchange, _settings.RoutingKey, arguments: null);
			_declaredQueues.TryAdd(_settings.Queue, true);
			_logger.LogInformation($"Queue '{_settings.Queue}' declared and bound to exchange '{_settings.Exchange}' with routing key '{_settings.RoutingKey}'.");
		}
	}
	protected async Task<IConnection> CreateConnectionAsync()
	{
		var factory = new ConnectionFactory
		{
			HostName = _settings.HostName,
			UserName = _settings.UserName,
			Password = _settings.Password,
			VirtualHost = _settings.VirtualHost,
			Port = _settings.Port,
			ClientProvidedName = _settings.ClientProvidedName,
		};

		return await factory.CreateConnectionAsync();
	}
}