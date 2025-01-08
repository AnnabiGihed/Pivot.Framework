namespace Templates.Core.Infrastructure.Persistence.EntityFrameworkCore.Outbox;

public sealed class OutboxMessage
{
	public Guid Id { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public string EventType { get; set; }
	public string Payload { get; set; }
	public bool Processed { get; set; } = false;
	public DateTime? ProcessedAtUtc { get; set; }
	public int RetryCount { get; set; } = 0; // Retry mechanism
}