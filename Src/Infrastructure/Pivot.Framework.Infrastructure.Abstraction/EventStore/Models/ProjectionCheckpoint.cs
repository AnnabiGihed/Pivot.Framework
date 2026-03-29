namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Tracks the last processed event position for a named projection.
///              Enables resumable projection rebuilds and incremental catch-up processing.
/// </summary>
public sealed class ProjectionCheckpoint
{
	/// <summary>
	/// Unique identifier of the checkpoint record.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// The name of the projection (e.g., "OrderSummaryProjection").
	/// </summary>
	public string ProjectionName { get; init; } = string.Empty;

	/// <summary>
	/// The version of the projection schema. Used for version-suffixed table rebuilds.
	/// </summary>
	public int ProjectionVersion { get; init; }

	/// <summary>
	/// The position (sequence number or timestamp) of the last successfully processed event.
	/// </summary>
	public long LastProcessedPosition { get; set; }

	/// <summary>
	/// The EventId of the last successfully processed event.
	/// </summary>
	public Guid? LastProcessedEventId { get; set; }

	/// <summary>
	/// UTC timestamp of the last checkpoint update.
	/// </summary>
	public DateTime LastUpdatedUtc { get; set; }
}
