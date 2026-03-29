using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Integration event emitted when a projection version is promoted.
///              Read services consume this to execute the checkpoint cut-over protocol:
///              seed the runtime checkpoint at LastEventPosition and switch to the new version.
/// </summary>
public sealed record ProjectionVersionPromotedEvent : IIntegrationEvent
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
	public string? CorrelationId { get; init; }
	public string ProjectionName { get; init; } = string.Empty;
	public int OldVersion { get; init; }
	public int NewVersion { get; init; }
	public long LastEventPosition { get; init; }
}
