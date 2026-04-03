using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using RabbitMQ.Client;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessagePublisher;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.Resilience;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for route resolution in <see cref="RabbitMQPublisher"/>.
///              Verifies fallback routing, resolver-based overrides, and per-message routing
///              variation without requiring a live RabbitMQ broker.
/// </summary>
public class RabbitMQPublisherRoutingTests
{
	#region Test Infrastructure

	private sealed class TestRabbitMQPublisher : RabbitMQPublisher
	{
		public List<OutboxRoute> PublishedRoutes { get; } = [];

		public TestRabbitMQPublisher(
			IOptions<RabbitMQSettings> options,
			IMessageCompressor compressor,
			IMessageEncryptor encryptor,
			MessagingResiliencePolicies resiliencePolicies,
			IOutboxRoutingResolver? routingResolver = null)
			: base(options, compressor, encryptor, resiliencePolicies, routingResolver)
		{
		}

		protected override Task EnsureConnectionAsync() => Task.CompletedTask;

		protected override Task PublishToChannelAsync(OutboxRoute route, BasicProperties properties, byte[] encryptedMessage)
		{
			PublishedRoutes.Add(route);
			return Task.CompletedTask;
		}
	}

	private sealed class MessageTypeRoutingResolver : IOutboxRoutingResolver
	{
		public OutboxRoute Resolve(OutboxMessage message)
		{
			return message.EventType switch
			{
				"OrderCreated" => new OutboxRoute("orders.exchange", "orders.created"),
				"InvoiceRaised" => new OutboxRoute("billing.exchange", "billing.raised"),
				_ => new OutboxRoute("fallback.exchange", "fallback.route")
			};
		}
	}

	private static RabbitMQSettings CreateSettings() => new()
	{
		HostName = "localhost",
		UserName = "guest",
		Password = "guest",
		VirtualHost = "/",
		Port = 5672,
		Queue = "pivot.queue",
		Exchange = "pivot.exchange",
		RoutingKey = "pivot.default",
		EncryptionKey = "12345678901234561234567890123456",
		ClientProvidedName = "pivot-tests"
	};

	private static MessagingResiliencePolicies CreatePolicies()
	{
		var retry = Policy.Handle<Exception>().RetryAsync(0);
		var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(1));
		return new MessagingResiliencePolicies(retry, breaker);
	}

	private static (IMessageCompressor Compressor, IMessageEncryptor Encryptor) CreateSerializationDependencies()
	{
		var compressor = Substitute.For<IMessageCompressor>();
		compressor.Compress(Arg.Any<byte[]>()).Returns(ci => ci.Arg<byte[]>());

		var encryptor = Substitute.For<IMessageEncryptor>();
		encryptor.Encrypt(Arg.Any<byte[]>()).Returns(ci => ci.Arg<byte[]>());

		return (compressor, encryptor);
	}

	#endregion

	[Fact]
	public async Task RabbitMQPublisher_UsesResolverRoute_WhenResolverRegistered()
	{
		var settings = Options.Create(CreateSettings());
		var (compressor, encryptor) = CreateSerializationDependencies();
		var publisher = new TestRabbitMQPublisher(
			settings,
			compressor,
			encryptor,
			CreatePolicies(),
			new MessageTypeRoutingResolver());

		var result = await publisher.PublishAsync(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "OrderCreated"
		});

		result.IsSuccess.Should().BeTrue();
		publisher.PublishedRoutes.Should().ContainSingle();
		publisher.PublishedRoutes[0].Should().Be(new OutboxRoute("orders.exchange", "orders.created"));
	}

	[Fact]
	public async Task RabbitMQPublisher_UsesFallbackSettings_WhenResolverMissing()
	{
		var settings = Options.Create(CreateSettings());
		var (compressor, encryptor) = CreateSerializationDependencies();
		var publisher = new TestRabbitMQPublisher(
			settings,
			compressor,
			encryptor,
			CreatePolicies());

		var result = await publisher.PublishAsync(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "OrderCreated"
		});

		result.IsSuccess.Should().BeTrue();
		publisher.PublishedRoutes.Should().ContainSingle();
		publisher.PublishedRoutes[0].Should().Be(new OutboxRoute("pivot.exchange", "pivot.default"));
	}

	[Fact]
	public async Task RabbitMQPublisher_PublishesDifferentMessagesWithDifferentRoutingKeys()
	{
		var settings = Options.Create(CreateSettings());
		var (compressor, encryptor) = CreateSerializationDependencies();
		var publisher = new TestRabbitMQPublisher(
			settings,
			compressor,
			encryptor,
			CreatePolicies(),
			new MessageTypeRoutingResolver());

		await publisher.PublishAsync(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "OrderCreated"
		});

		await publisher.PublishAsync(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "InvoiceRaised"
		});

		publisher.PublishedRoutes.Should().HaveCount(2);
		publisher.PublishedRoutes[0].Should().Be(new OutboxRoute("orders.exchange", "orders.created"));
		publisher.PublishedRoutes[1].Should().Be(new OutboxRoute("billing.exchange", "billing.raised"));
	}
}
