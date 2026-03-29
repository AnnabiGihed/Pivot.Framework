using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Topology;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Topology;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="TopologyOptions"/> and <see cref="ExchangeBinding"/>.
///              Verifies binding defaults, topology registration, and chaining support.
/// </summary>
public class TopologyOptionsTests
{
	#region ExchangeBinding Tests

	[Fact]
	public void ExchangeBinding_ShouldHaveDefaults()
	{
		var binding = new ExchangeBinding
		{
			Exchange = "mdm.events",
			Queue = "mastering-queue",
			RoutingKey = "mastering.#"
		};

		binding.ExchangeType.Should().Be("topic");
		binding.EnableDeadLetterQueue.Should().BeTrue();
		binding.MaxRetryCount.Should().Be(3);
		binding.RetryDelayMs.Should().Be(5000);
		binding.UseQuorumQueue.Should().BeTrue();
		binding.ConsumerName.Should().BeNull();
	}

	[Fact]
	public void ExchangeBinding_ShouldSetAllProperties()
	{
		var binding = new ExchangeBinding
		{
			Exchange = "events",
			ExchangeType = "direct",
			Queue = "q1",
			RoutingKey = "key1",
			ConsumerName = "consumer-1",
			EnableDeadLetterQueue = false,
			MaxRetryCount = 5,
			RetryDelayMs = 10000,
			UseQuorumQueue = false
		};

		binding.Exchange.Should().Be("events");
		binding.ExchangeType.Should().Be("direct");
		binding.Queue.Should().Be("q1");
		binding.RoutingKey.Should().Be("key1");
		binding.ConsumerName.Should().Be("consumer-1");
		binding.EnableDeadLetterQueue.Should().BeFalse();
		binding.MaxRetryCount.Should().Be(5);
		binding.RetryDelayMs.Should().Be(10000);
		binding.UseQuorumQueue.Should().BeFalse();
	}

	#endregion

	#region TopologyOptions Tests

	[Fact]
	public void TopologyOptions_ShouldStartEmpty()
	{
		var options = new TopologyOptions();

		options.Bindings.Should().BeEmpty();
	}

	[Fact]
	public void Bind_ShouldAddBindingAndSupportChaining()
	{
		var options = new TopologyOptions()
			.Bind(new ExchangeBinding { Exchange = "ex1", Queue = "q1", RoutingKey = "r1" })
			.Bind(new ExchangeBinding { Exchange = "ex2", Queue = "q2", RoutingKey = "r2" });

		options.Bindings.Should().HaveCount(2);
		options.Bindings[0].Exchange.Should().Be("ex1");
		options.Bindings[1].Exchange.Should().Be("ex2");
	}

	#endregion
}
