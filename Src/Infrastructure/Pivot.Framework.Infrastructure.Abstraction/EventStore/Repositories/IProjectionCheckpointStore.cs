using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Contract for managing projection checkpoint persistence.
///              Enables resumable projection processing and rebuild operations.
/// </summary>
public interface IProjectionCheckpointStore
{
	/// <summary>
	/// Gets the checkpoint for a named projection at a specific version.
	/// Returns null if no checkpoint exists (projection has never been processed).
	/// </summary>
	Task<ProjectionCheckpoint?> GetCheckpointAsync(
		string projectionName,
		int projectionVersion,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates or updates the checkpoint for a named projection.
	/// </summary>
	Task<Result> SaveCheckpointAsync(
		ProjectionCheckpoint checkpoint,
		CancellationToken cancellationToken = default);
}
