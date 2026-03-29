using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IProjectionCheckpointStore"/>.
///              Manages projection checkpoint persistence for resumable processing.
/// </summary>
public sealed class ProjectionCheckpointStore(DbContext dbContext) : IProjectionCheckpointStore
{
	private readonly DbContext _dbContext = dbContext;

	/// <inheritdoc />
	public async Task<ProjectionCheckpoint?> GetCheckpointAsync(
		string projectionName,
		int projectionVersion,
		CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<ProjectionCheckpoint>()
			.FirstOrDefaultAsync(
				c => c.ProjectionName == projectionName && c.ProjectionVersion == projectionVersion,
				cancellationToken);
	}

	/// <inheritdoc />
	public async Task<Result> SaveCheckpointAsync(
		ProjectionCheckpoint checkpoint,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var existing = await _dbContext.Set<ProjectionCheckpoint>()
				.FirstOrDefaultAsync(
					c => c.ProjectionName == checkpoint.ProjectionName
						&& c.ProjectionVersion == checkpoint.ProjectionVersion,
					cancellationToken);

			if (existing is null)
			{
				_dbContext.Set<ProjectionCheckpoint>().Add(checkpoint);
			}
			else
			{
				existing.LastProcessedPosition = checkpoint.LastProcessedPosition;
				existing.LastProcessedEventId = checkpoint.LastProcessedEventId;
				existing.LastUpdatedUtc = checkpoint.LastUpdatedUtc;
			}

			await _dbContext.SaveChangesAsync(cancellationToken);
			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("ProjectionCheckpoint.SaveError", ex.Message));
		}
	}
}
