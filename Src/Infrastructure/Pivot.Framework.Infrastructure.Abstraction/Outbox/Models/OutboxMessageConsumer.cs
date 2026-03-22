namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Represents a consumer that has processed a specific outbox message.
///              Used to track idempotent message consumption.
/// </summary>
public sealed class OutboxMessageConsumer
{
	#region Properties

	/// <summary>
	/// The unique identifier of the outbox message that was consumed.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// The name of the consumer that processed the message.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	#endregion
}
