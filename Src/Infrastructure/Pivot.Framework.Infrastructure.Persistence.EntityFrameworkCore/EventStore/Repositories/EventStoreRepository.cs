using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IEventStoreRepository{TContext}"/>.
///              Provides append-only persistence and querying for event history entries.
///              Events are added to the EF Core change tracker and committed in the same
///              transaction as business state and outbox messages.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext type that contains the event store table.</typeparam>
public sealed class EventStoreRepository<TContext>(TContext dbContext) : IEventStoreRepository<TContext>
	where TContext : DbContext, IPersistenceContext
{
	private readonly TContext _dbContext = dbContext;

	/// <inheritdoc />
	public Task<Result> AppendAsync(EventHistoryEntry entry, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<EventHistoryEntry>().Add(entry);
		return Task.FromResult(Result.Success());
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<EventHistoryEntry>> GetByAggregateIdAsync(
		string aggregateId,
		string aggregateType,
		CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<EventHistoryEntry>()
			.Where(e => e.AggregateId == aggregateId && e.AggregateType == aggregateType)
			.OrderBy(e => e.AggregateVersion)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<EventHistoryEntry>> GetByAggregateIdFromVersionAsync(
		string aggregateId,
		string aggregateType,
		int fromVersion,
		CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<EventHistoryEntry>()
			.Where(e => e.AggregateId == aggregateId
				&& e.AggregateType == aggregateType
				&& e.AggregateVersion >= fromVersion)
			.OrderBy(e => e.AggregateVersion)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<EventHistoryEntry>> GetFromPositionAsync(
		long fromPosition,
		int maxCount,
		CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<EventHistoryEntry>()
			.OrderBy(e => e.CreatedAtUtc)
			.ThenBy(e => e.Id)
			.Skip((int)fromPosition)
			.Take(maxCount)
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task<long> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<EventHistoryEntry>()
			.LongCountAsync(cancellationToken);
	}
}
