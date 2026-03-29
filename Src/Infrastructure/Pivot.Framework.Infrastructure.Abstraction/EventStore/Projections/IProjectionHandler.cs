using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Projections;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Contract for a projection handler that processes event history entries
///              to update read models/materialized views.
/// </summary>
public interface IProjectionHandler
{
	/// <summary>
	/// The unique name of this projection.
	/// </summary>
	string ProjectionName { get; }

	/// <summary>
	/// The current version of this projection's schema.
	/// Incrementing this triggers a rebuild with version-suffixed deduplication.
	/// </summary>
	int ProjectionVersion { get; }

	/// <summary>
	/// Processes a single event history entry to update the projection.
	/// </summary>
	Task HandleAsync(EventHistoryEntry entry, CancellationToken cancellationToken = default);
}
