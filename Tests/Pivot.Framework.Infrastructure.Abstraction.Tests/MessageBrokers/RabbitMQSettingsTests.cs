using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.MessageBrokers;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="RabbitMQSettings"/>.
///              Verifies property access and assignment.
/// </summary>
public class RabbitMQSettingsTests
{
	#region Property Tests
	/// <summary>
	/// Verifies that all required properties can be set and retrieved.
	/// </summary>
	[Fact]
	public void Properties_ShouldGetAndSet()
	{
		var settings = new RabbitMQSettings
		{
			Port = 5672,
			Queue = "test-queue",
			HostName = "localhost",
			UserName = "guest",
			Password = "guest",
			Exchange = "test-exchange",
			RoutingKey = "test.routing.key",
			VirtualHost = "/",
			EncryptionKey = "encryption-key-123",
			ClientProvidedName = "test-client"
		};

		settings.Port.Should().Be(5672);
		settings.Queue.Should().Be("test-queue");
		settings.HostName.Should().Be("localhost");
		settings.UserName.Should().Be("guest");
		settings.Password.Should().Be("guest");
		settings.Exchange.Should().Be("test-exchange");
		settings.RoutingKey.Should().Be("test.routing.key");
		settings.VirtualHost.Should().Be("/");
		settings.EncryptionKey.Should().Be("encryption-key-123");
		settings.ClientProvidedName.Should().Be("test-client");
	}
	#endregion
}
