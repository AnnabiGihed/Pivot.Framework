using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.Audit;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Abstraction for persisting explicit audit log entries for high-risk
///              administrative and steward actions. Distinct from the event store which
///              tracks domain events — this captures user-initiated admin operations
///              (activation, replay start, service client registration, etc.).
/// </summary>
public interface IAuditLogService
{
	/// <summary>Records an audit log entry.</summary>
	Task<Result> LogAsync(AuditEntry entry, CancellationToken ct = default);

	/// <summary>Queries audit log entries by actor, action, or resource.</summary>
	Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query, CancellationToken ct = default);
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Represents a single audit log entry for an administrative action.
/// </summary>
public sealed class AuditEntry
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public required string ActorId { get; init; }
	public string? ActorName { get; init; }
	public required string Action { get; init; }
	public required string ResourceType { get; init; }
	public string? ResourceId { get; init; }
	public string? Details { get; init; }
	public string? CorrelationId { get; init; }
	public string? IpAddress { get; init; }
	public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Query parameters for filtering audit log entries.
/// </summary>
public sealed class AuditQuery
{
	public string? ActorId { get; init; }
	public string? Action { get; init; }
	public string? ResourceType { get; init; }
	public string? ResourceId { get; init; }
	public DateTime? FromUtc { get; init; }
	public DateTime? ToUtc { get; init; }
	public int Skip { get; init; }
	public int Take { get; init; } = 50;
}
