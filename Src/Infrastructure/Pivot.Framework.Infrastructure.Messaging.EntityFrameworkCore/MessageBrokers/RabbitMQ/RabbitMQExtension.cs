using Polly;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageReceiver;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Services;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.Resilience;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageReceiver;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageEncryptor;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageCompressor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Added registration for <see cref="IIntegrationEventPublisher"/>
///              to support first-class integration events alongside domain events.
/// Purpose     : DI registration extensions for the RabbitMQ messaging infrastructure.
///              Registers the message publisher, receiver, compressor, encryptor, serializer,
///              and resilience policies. Transport-agnostic outbox persistence is registered
///              separately via the write-side persistence extensions.
/// </summary>
public static class RabbitMQPublisherExtensions
{
	/// <summary>
	/// Registers all RabbitMQ messaging infrastructure services including the publisher,
	/// receiver hosted service, compressor, encryptor, and Polly resilience policies.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="configuration">The application configuration containing the "RabbitMQ" section.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<RabbitMQSettings>(options => configuration.GetSection("RabbitMQ").Bind(options));

		services.AddLogging();

		#region Polly
		services.AddSingleton(provider =>
		{
			var retryPolicy = Policy.Handle<Exception>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

			var circuitBreakerPolicy = Policy.Handle<Exception>()
				.CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

			return new MessagingResiliencePolicies(retryPolicy, circuitBreakerPolicy);
		});
		#endregion

		#region Message Broker
		services.AddSingleton<IMessageEncryptor>(provider =>
		{
			var settings = provider.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
			return new AesMessageEncryptor(settings.EncryptionKey);
		});
		services.AddSingleton<IMessageCompressor, GZipMessageCompressor>();
		services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
		services.AddSingleton<IMessageReceiver, RabbitMQReceiver>();
		services.AddHostedService<RabbitMQReceiverHostedService>();
		#endregion
		return services;
	}
}
