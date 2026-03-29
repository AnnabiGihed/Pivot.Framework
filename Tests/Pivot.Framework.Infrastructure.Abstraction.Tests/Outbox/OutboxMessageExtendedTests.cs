using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for the new <see cref="OutboxMessage"/> properties added for
///              enterprise outbox/inbox support: CorrelationId, Kind, FailedAtUtc, LastError.
/// </summary>
public class OutboxMessageExtendedTests
{
	#region Default Value Tests

	/// <summary>
	/// Verifies that <see cref="OutboxMessage.Kind"/> defaults to DomainEvent for backward compatibility.
	/// </summary>
	[Fact]
	public void OutboxMessage_Kind_ShouldDefaultToDomainEvent()
	{
		var msg = new OutboxMessage();

		msg.Kind.Should().Be(MessageKind.DomainEvent);
	}

	/// <summary>
	/// Verifies that <see cref="OutboxMessage.CorrelationId"/> defaults to null.
	/// </summary>
	[Fact]
	public void OutboxMessage_CorrelationId_ShouldDefaultToNull()
	{
		var msg = new OutboxMessage();

		msg.CorrelationId.Should().BeNull();
	}

	/// <summary>
	/// Verifies that <see cref="OutboxMessage.FailedAtUtc"/> defaults to null.
	/// </summary>
	[Fact]
	public void OutboxMessage_FailedAtUtc_ShouldDefaultToNull()
	{
		var msg = new OutboxMessage();

		msg.FailedAtUtc.Should().BeNull();
	}

	/// <summary>
	/// Verifies that <see cref="OutboxMessage.LastError"/> defaults to null.
	/// </summary>
	[Fact]
	public void OutboxMessage_LastError_ShouldDefaultToNull()
	{
		var msg = new OutboxMessage();

		msg.LastError.Should().BeNull();
	}

	#endregion

	#region Property Assignment Tests

	/// <summary>
	/// Verifies that all new properties can be set and retrieved.
	/// </summary>
	[Fact]
	public void OutboxMessage_NewProperties_ShouldBeSettableAndGettable()
	{
		var now = DateTime.UtcNow;

		var msg = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			CorrelationId = "corr-789",
			Kind = MessageKind.IntegrationEvent,
			FailedAtUtc = now,
			LastError = "Broker unreachable"
		};

		msg.CorrelationId.Should().Be("corr-789");
		msg.Kind.Should().Be(MessageKind.IntegrationEvent);
		msg.FailedAtUtc.Should().Be(now);
		msg.LastError.Should().Be("Broker unreachable");
	}

	#endregion

	#region MessageKind Tests

	/// <summary>
	/// Verifies that MessageKind has the expected values.
	/// </summary>
	[Fact]
	public void MessageKind_ShouldHaveExpectedValues()
	{
		((int)MessageKind.DomainEvent).Should().Be(0);
		((int)MessageKind.IntegrationEvent).Should().Be(1);
		Enum.GetNames<MessageKind>().Should().HaveCount(2);
	}

	#endregion
}
