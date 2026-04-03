using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.InProcess;

namespace Pivot.Framework.Containers.API.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Unit tests for <see cref="InProcessMessagePublisher"/> behaviour that is
///              relevant to integration-event mapping. Verifies that integration-event
///              outbox messages fail fast with a clear configuration error.
/// </summary>
public class InProcessMessagePublisherTests
{
	[Fact]
	public async Task PublishAsync_IntegrationEventMessage_ShouldReturnClearFailure()
	{
		var services = new ServiceCollection().BuildServiceProvider();
		var logger = Substitute.For<ILogger<InProcessMessagePublisher>>();
		var publisher = new InProcessMessagePublisher(services, logger);

		var result = await publisher.PublishAsync(new OutboxMessage
		{
			Id = Guid.NewGuid(),
			Payload = "{}",
			EventType = "TestIntegrationEvent",
			Kind = MessageKind.IntegrationEvent
		});

		result.IsFailure.Should().BeTrue();
		result.Error!.Code.Should().Be("InProcessPublisher.IntegrationEventsNotSupported");
	}
}
