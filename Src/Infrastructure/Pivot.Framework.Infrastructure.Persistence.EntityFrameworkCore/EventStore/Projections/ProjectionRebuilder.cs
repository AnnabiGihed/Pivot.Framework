using Microsoft.Extensions.Logging;
using Pivot.Framework.Application.Abstractions.Replay;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Projections;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Projections;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Default implementation of <see cref="IProjectionRebuilder"/>.
///              Replays events from the event store through a projection handler in batches,
///              with checkpoint tracking for resume support. Sets the ReplayContext flag
///              to suppress non-projection side effects during rebuild.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that owns the event store.</typeparam>
public sealed class ProjectionRebuilder<TContext> : IProjectionRebuilder
	where TContext : IPersistenceContext
{
	private readonly IEventStoreRepository<TContext> _eventStoreRepository;
	private readonly IProjectionCheckpointStore _checkpointStore;
	private readonly ILogger<ProjectionRebuilder<TContext>> _logger;

	public ProjectionRebuilder(
		IEventStoreRepository<TContext> eventStoreRepository,
		IProjectionCheckpointStore checkpointStore,
		ILogger<ProjectionRebuilder<TContext>> logger)
	{
		_eventStoreRepository = eventStoreRepository ?? throw new ArgumentNullException(nameof(eventStoreRepository));
		_checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task<Result> RebuildAsync(
		IProjectionHandler projectionHandler,
		int batchSize = 1000,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(projectionHandler);

		_logger.LogInformation(
			"Starting projection rebuild for '{ProjectionName}' v{Version}",
			projectionHandler.ProjectionName,
			projectionHandler.ProjectionVersion);

		try
		{
			// Load existing checkpoint for resume support
			var checkpoint = await _checkpointStore.GetCheckpointAsync(
				projectionHandler.ProjectionName,
				projectionHandler.ProjectionVersion,
				cancellationToken);

			var currentPosition = checkpoint?.LastProcessedPosition ?? 0;

			using (ReplayContext.BeginReplayScope())
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var events = await _eventStoreRepository.GetFromPositionAsync(
						currentPosition,
						batchSize,
						cancellationToken);

					if (events.Count == 0)
						break;

					foreach (var entry in events)
					{
						await projectionHandler.HandleAsync(entry, cancellationToken);
					}

					currentPosition += events.Count;

					// Save checkpoint after each batch
					var newCheckpoint = new ProjectionCheckpoint
					{
						Id = checkpoint?.Id ?? Guid.NewGuid(),
						ProjectionName = projectionHandler.ProjectionName,
						ProjectionVersion = projectionHandler.ProjectionVersion,
						LastProcessedPosition = currentPosition,
						LastProcessedEventId = events[^1].Id,
						LastUpdatedUtc = DateTime.UtcNow
					};

					var saveResult = await _checkpointStore.SaveCheckpointAsync(newCheckpoint, cancellationToken);
					if (saveResult.IsFailure)
					{
						_logger.LogError("Failed to save projection checkpoint at position {Position}: {Error}",
							currentPosition, saveResult.Error);
						return saveResult;
					}

					checkpoint = newCheckpoint;

					_logger.LogDebug(
						"Projection '{ProjectionName}' v{Version} processed batch, position: {Position}",
						projectionHandler.ProjectionName,
						projectionHandler.ProjectionVersion,
						currentPosition);
				}
			}

			_logger.LogInformation(
				"Projection rebuild completed for '{ProjectionName}' v{Version}. Final position: {Position}",
				projectionHandler.ProjectionName,
				projectionHandler.ProjectionVersion,
				currentPosition);

			return Result.Success();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex,
				"Projection rebuild failed for '{ProjectionName}' v{Version}",
				projectionHandler.ProjectionName,
				projectionHandler.ProjectionVersion);
			return Result.Failure(new Error("Projection.RebuildFailed", ex.Message));
		}
	}
}
