using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Projections;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Contract for rebuilding projections from the event store.
///              Replays all events through a projection handler, using checkpoints
///              for resume support and version-namespaced deduplication keys.
/// </summary>
public interface IProjectionRebuilder
{
	/// <summary>
	/// Rebuilds a projection by replaying all events from the event store.
	/// Sets <see cref="Application.Abstractions.Replay.ReplayContext.IsReplaying"/> to true
	/// during the rebuild to suppress non-projection side effects.
	/// </summary>
	/// <param name="projectionHandler">The projection handler to process events through.</param>
	/// <param name="batchSize">Number of events to process per batch. Defaults to 1000.</param>
	/// <param name="cancellationToken">Token for cooperative cancellation.</param>
	Task<Result> RebuildAsync(
		IProjectionHandler projectionHandler,
		int batchSize = 1000,
		CancellationToken cancellationToken = default);
}
