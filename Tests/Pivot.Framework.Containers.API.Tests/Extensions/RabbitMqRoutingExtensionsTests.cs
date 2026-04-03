using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Publishing;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.RabbitMQ.Routing;

namespace Pivot.Framework.Containers.API.Tests.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="RabbitMqRoutingExtensions"/>.
///              Verifies that custom outbox routing resolvers are registered with a
///              singleton lifetime compatible with the singleton RabbitMQ publisher.
/// </summary>
public class RabbitMqRoutingExtensionsTests
{
	private sealed class TestResolver : IOutboxRoutingResolver
	{
		public OutboxRoute Resolve(OutboxMessage message) => new("test.exchange", "test.key");
	}

	[Fact]
	public void AddOutboxRoutingResolver_ShouldRegisterSingletonResolver()
	{
		var services = new ServiceCollection();

		services.AddOutboxRoutingResolver<TestResolver>();

		var descriptor = services.SingleOrDefault(x => x.ServiceType == typeof(IOutboxRoutingResolver));

		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TestResolver));
		descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
	}

	[Fact]
	public void AddOutboxRoutingResolver_CalledTwice_ShouldNotDuplicate()
	{
		var services = new ServiceCollection();

		services.AddOutboxRoutingResolver<TestResolver>();
		services.AddOutboxRoutingResolver<TestResolver>();

		services.Count(x => x.ServiceType == typeof(IOutboxRoutingResolver)).Should().Be(1);
	}
}
