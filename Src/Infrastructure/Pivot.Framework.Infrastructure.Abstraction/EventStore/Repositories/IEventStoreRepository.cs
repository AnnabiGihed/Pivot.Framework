using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Repository contract for the append-only event store.
///              Provides methods to persist event history entries and query event streams
///              by aggregate or globally. Scoped by DbContext type for multi-context support.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that owns the event store table.</typeparam>
public interface IEventStoreRepository<TContext>
	where TContext : IPersistenceContext
{
	/// <summary>
	/// Appends an event history entry to the event store within the current transaction.
	/// </summary>
	Task<Result> AppendAsync(EventHistoryEntry entry, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all events for a specific aggregate, ordered by aggregate version.
	/// </summary>
	Task<IReadOnlyList<EventHistoryEntry>> GetByAggregateIdAsync(
		string aggregateId,
		string aggregateType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all events for a specific aggregate starting from a given version.
	/// </summary>
	Task<IReadOnlyList<EventHistoryEntry>> GetByAggregateIdFromVersionAsync(
		string aggregateId,
		string aggregateType,
		int fromVersion,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves events globally from a given position, ordered by creation time.
	/// Used for projection catch-up and rebuild.
	/// </summary>
	/// <param name="fromPosition">The sequence position to start reading from (exclusive).</param>
	/// <param name="maxCount">Maximum number of events to return.</param>
	Task<IReadOnlyList<EventHistoryEntry>> GetFromPositionAsync(
		long fromPosition,
		int maxCount,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the current maximum global sequence position in the event store.
	/// </summary>
	Task<long> GetCurrentPositionAsync(CancellationToken cancellationToken = default);
}
