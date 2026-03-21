using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxMessage"/> and <see cref="OutboxMessageConsumer"/>.
///              Verifies default values and property assignment.
/// </summary>
public class OutboxMessageTests
{
	#region OutboxMessage Default Values Tests
	/// <summary>
	/// Verifies that <see cref="OutboxMessage.RetryCount"/> defaults to 0.
	/// </summary>
	[Fact]
	public void OutboxMessage_RetryCount_ShouldDefaultToZero()
	{
		var msg = new OutboxMessage();

		msg.RetryCount.Should().Be(0);
	}

	/// <summary>
	/// Verifies that <see cref="OutboxMessage.Processed"/> defaults to false.
	/// </summary>
	[Fact]
	public void OutboxMessage_Processed_ShouldDefaultToFalse()
	{
		var msg = new OutboxMessage();

		msg.Processed.Should().BeFalse();
	}

	/// <summary>
	/// Verifies that all properties can be set and retrieved.
	/// </summary>
	[Fact]
	public void OutboxMessage_Properties_ShouldBeSettableAndGettable()
	{
		var id = Guid.NewGuid();
		var now = DateTime.UtcNow;

		var msg = new OutboxMessage
		{
			Id = id,
			Payload = "{\"key\":\"value\"}",
			EventType = "OrderCreated",
			RetryCount = 3,
			CreatedAtUtc = now,
			Processed = true,
			ProcessedAtUtc = now.AddMinutes(5)
		};

		msg.Id.Should().Be(id);
		msg.Payload.Should().Be("{\"key\":\"value\"}");
		msg.EventType.Should().Be("OrderCreated");
		msg.RetryCount.Should().Be(3);
		msg.CreatedAtUtc.Should().Be(now);
		msg.Processed.Should().BeTrue();
		msg.ProcessedAtUtc.Should().Be(now.AddMinutes(5));
	}
	#endregion

	#region OutboxMessageConsumer Tests
	/// <summary>
	/// Verifies that <see cref="OutboxMessageConsumer.Name"/> defaults to empty string.
	/// </summary>
	[Fact]
	public void OutboxMessageConsumer_Name_ShouldDefaultToEmpty()
	{
		var consumer = new OutboxMessageConsumer();

		consumer.Name.Should().BeEmpty();
	}

	/// <summary>
	/// Verifies that all OutboxMessageConsumer properties can be set.
	/// </summary>
	[Fact]
	public void OutboxMessageConsumer_Properties_ShouldBeSettable()
	{
		var id = Guid.NewGuid();

		var consumer = new OutboxMessageConsumer
		{
			Id = id,
			Name = "PaymentService"
		};

		consumer.Id.Should().Be(id);
		consumer.Name.Should().Be("PaymentService");
	}
	#endregion
}
