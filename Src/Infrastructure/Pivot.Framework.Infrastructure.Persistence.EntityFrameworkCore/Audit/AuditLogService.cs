using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Audit;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Audit;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IAuditLogService"/>.
///              Persists explicit audit log entries for high-risk administrative actions
///              to a dedicated AuditLog table.
/// </summary>
public sealed class AuditLogService(DbContext dbContext) : IAuditLogService
{
	private readonly DbContext _dbContext = dbContext;

	/// <inheritdoc />
	public async Task<Result> LogAsync(AuditEntry entry, CancellationToken ct = default)
	{
		try
		{
			_dbContext.Set<AuditEntry>().Add(entry);
			await _dbContext.SaveChangesAsync(ct);
			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("AuditLog.WriteFailed", ex.Message));
		}
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query, CancellationToken ct = default)
	{
		var q = _dbContext.Set<AuditEntry>().AsNoTracking().AsQueryable();

		if (!string.IsNullOrEmpty(query.ActorId))
			q = q.Where(e => e.ActorId == query.ActorId);
		if (!string.IsNullOrEmpty(query.Action))
			q = q.Where(e => e.Action == query.Action);
		if (!string.IsNullOrEmpty(query.ResourceType))
			q = q.Where(e => e.ResourceType == query.ResourceType);
		if (!string.IsNullOrEmpty(query.ResourceId))
			q = q.Where(e => e.ResourceId == query.ResourceId);
		if (query.FromUtc.HasValue)
			q = q.Where(e => e.OccurredAtUtc >= query.FromUtc.Value);
		if (query.ToUtc.HasValue)
			q = q.Where(e => e.OccurredAtUtc <= query.ToUtc.Value);

		return await q.OrderByDescending(e => e.OccurredAtUtc)
			.Skip(query.Skip)
			.Take(query.Take)
			.ToListAsync(ct);
	}
}
