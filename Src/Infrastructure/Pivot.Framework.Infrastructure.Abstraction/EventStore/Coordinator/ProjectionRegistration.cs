namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Represents a registered projection in the Projection Coordinator.
///              Tracks the projection name, current active version, rebuild state,
///              and parity check results.
/// </summary>
public sealed class ProjectionRegistration
{
	public Guid Id { get; init; }
	public string ProjectionName { get; init; } = string.Empty;
	public int ActiveVersion { get; set; }
	public int? RebuildTargetVersion { get; set; }
	public ProjectionState State { get; set; } = ProjectionState.Active;
	public long? RebuildFinalPosition { get; set; }
	public DateTime? LastParityCheckUtc { get; set; }
	public bool? ParityCheckPassed { get; set; }
	public DateTime CreatedAtUtc { get; init; }
	public DateTime LastUpdatedUtc { get; set; }
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Lifecycle states for a projection registration.
/// </summary>
public enum ProjectionState
{
	Active,
	RebuildPlanned,
	Rebuilding,
	ParityChecking,
	PromotionReady,
	Deprecated
}
