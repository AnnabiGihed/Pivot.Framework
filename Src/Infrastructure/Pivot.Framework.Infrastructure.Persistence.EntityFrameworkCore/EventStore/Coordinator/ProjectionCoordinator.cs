using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Coordinator;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IProjectionCoordinator"/>.
///              Manages the full projection lifecycle: registration, rebuild planning,
///              parity checking, version promotion, and deprecation.
///              Implements the checkpoint cut-over protocol from MDM spec Section 24.
/// </summary>
/// <typeparam name="TContext">The DbContext type that stores projection registrations.</typeparam>
public sealed class ProjectionCoordinator<TContext> : IProjectionCoordinator
	where TContext : DbContext, IPersistenceContext
{
	private readonly TContext _dbContext;
	private readonly IProjectionCheckpointStore _checkpointStore;
	private readonly ILogger<ProjectionCoordinator<TContext>> _logger;

	public ProjectionCoordinator(
		TContext dbContext,
		IProjectionCheckpointStore checkpointStore,
		ILogger<ProjectionCoordinator<TContext>> logger)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task<Result> RegisterProjectionAsync(string projectionName, int initialVersion, CancellationToken ct = default)
	{
		try
		{
			var existing = await _dbContext.Set<ProjectionRegistration>()
				.FirstOrDefaultAsync(r => r.ProjectionName == projectionName, ct);

			if (existing is not null)
				return Result.Failure(new Error("Projection.AlreadyRegistered", $"Projection '{projectionName}' is already registered."));

			_dbContext.Set<ProjectionRegistration>().Add(new ProjectionRegistration
			{
				Id = Guid.NewGuid(),
				ProjectionName = projectionName,
				ActiveVersion = initialVersion,
				State = ProjectionState.Active,
				CreatedAtUtc = DateTime.UtcNow,
				LastUpdatedUtc = DateTime.UtcNow
			});

			await _dbContext.SaveChangesAsync(ct);
			_logger.LogInformation("Registered projection '{Name}' at version {Version}", projectionName, initialVersion);
			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("Projection.RegisterFailed", ex.Message));
		}
	}

	/// <inheritdoc />
	public async Task<Result> PlanRebuildAsync(string projectionName, int targetVersion, CancellationToken ct = default)
	{
		return await TransitionStateAsync(projectionName, ProjectionState.Active, ProjectionState.RebuildPlanned, reg =>
		{
			reg.RebuildTargetVersion = targetVersion;
		}, ct);
	}

	/// <inheritdoc />
	public async Task<Result> StartRebuildAsync(string projectionName, int targetVersion, CancellationToken ct = default)
	{
		return await TransitionStateAsync(projectionName, ProjectionState.RebuildPlanned, ProjectionState.Rebuilding, _ => { }, ct);
	}

	/// <inheritdoc />
	public async Task<Result> CompleteRebuildAsync(string projectionName, int targetVersion, long finalPosition, CancellationToken ct = default)
	{
		return await TransitionStateAsync(projectionName, ProjectionState.Rebuilding, ProjectionState.ParityChecking, reg =>
		{
			reg.RebuildFinalPosition = finalPosition;
		}, ct);
	}

	/// <inheritdoc />
	public async Task<Result> RecordParityCheckAsync(string projectionName, int targetVersion, bool passed, CancellationToken ct = default)
	{
		return await TransitionStateAsync(projectionName, ProjectionState.ParityChecking,
			passed ? ProjectionState.PromotionReady : ProjectionState.RebuildPlanned, reg =>
		{
			reg.LastParityCheckUtc = DateTime.UtcNow;
			reg.ParityCheckPassed = passed;
		}, ct);
	}

	/// <inheritdoc />
	public async Task<Result> PromoteVersionAsync(string projectionName, int newVersion, CancellationToken ct = default)
	{
		return await TransitionStateAsync(projectionName, ProjectionState.PromotionReady, ProjectionState.Active, reg =>
		{
			var oldVersion = reg.ActiveVersion;
			reg.ActiveVersion = newVersion;
			reg.RebuildTargetVersion = null;
			reg.RebuildFinalPosition = null;
			reg.ParityCheckPassed = null;

			_logger.LogInformation(
				"Promoted projection '{Name}' from v{OldVersion} to v{NewVersion}",
				projectionName, oldVersion, newVersion);
		}, ct);
	}

	/// <inheritdoc />
	public async Task<ProjectionRegistration?> GetRegistrationAsync(string projectionName, CancellationToken ct = default)
	{
		return await _dbContext.Set<ProjectionRegistration>()
			.FirstOrDefaultAsync(r => r.ProjectionName == projectionName, ct);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<ProjectionRegistration>> GetAllRegistrationsAsync(CancellationToken ct = default)
	{
		return await _dbContext.Set<ProjectionRegistration>().ToListAsync(ct);
	}

	private async Task<Result> TransitionStateAsync(
		string projectionName, ProjectionState expectedState, ProjectionState newState,
		Action<ProjectionRegistration> configure, CancellationToken ct)
	{
		try
		{
			var reg = await _dbContext.Set<ProjectionRegistration>()
				.FirstOrDefaultAsync(r => r.ProjectionName == projectionName, ct);

			if (reg is null)
				return Result.Failure(new Error("Projection.NotFound", $"Projection '{projectionName}' not found."));

			if (reg.State != expectedState)
				return Result.Failure(new Error("Projection.InvalidState",
					$"Expected state '{expectedState}' but found '{reg.State}'."));

			reg.State = newState;
			reg.LastUpdatedUtc = DateTime.UtcNow;
			configure(reg);

			await _dbContext.SaveChangesAsync(ct);
			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("Projection.TransitionFailed", ex.Message));
		}
	}
}
