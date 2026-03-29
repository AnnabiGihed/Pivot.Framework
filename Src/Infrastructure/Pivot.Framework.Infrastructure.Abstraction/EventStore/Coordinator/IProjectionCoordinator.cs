using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Control plane for managing projection lifecycle: registration, rebuild planning,
///              parity checking, version promotion, and deprecation. Implements the checkpoint
///              cut-over protocol defined in MDM spec Section 24.
/// </summary>
public interface IProjectionCoordinator
{
	/// <summary>Registers a new projection with its initial version.</summary>
	Task<Result> RegisterProjectionAsync(string projectionName, int initialVersion, CancellationToken ct = default);

	/// <summary>Plans a rebuild for a projection at a new target version.</summary>
	Task<Result> PlanRebuildAsync(string projectionName, int targetVersion, CancellationToken ct = default);

	/// <summary>Marks a rebuild as started (transitions to Rebuilding state).</summary>
	Task<Result> StartRebuildAsync(string projectionName, int targetVersion, CancellationToken ct = default);

	/// <summary>Records the final position of a completed rebuild for parity checking.</summary>
	Task<Result> CompleteRebuildAsync(string projectionName, int targetVersion, long finalPosition, CancellationToken ct = default);

	/// <summary>Records the result of a parity check after rebuild completion.</summary>
	Task<Result> RecordParityCheckAsync(string projectionName, int targetVersion, bool passed, CancellationToken ct = default);

	/// <summary>
	/// Promotes a new projection version, emitting ProjectionVersionPromoted.
	/// Implements the cut-over protocol: seeds the service runtime checkpoint from the
	/// rebuild's final position and deprecates the old version.
	/// </summary>
	Task<Result> PromoteVersionAsync(string projectionName, int newVersion, CancellationToken ct = default);

	/// <summary>Gets the current registration for a named projection.</summary>
	Task<ProjectionRegistration?> GetRegistrationAsync(string projectionName, CancellationToken ct = default);

	/// <summary>Gets all registered projections.</summary>
	Task<IReadOnlyList<ProjectionRegistration>> GetAllRegistrationsAsync(CancellationToken ct = default);
}
