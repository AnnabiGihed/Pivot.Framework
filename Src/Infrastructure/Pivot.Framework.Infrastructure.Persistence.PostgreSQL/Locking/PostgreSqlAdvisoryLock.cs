using Microsoft.EntityFrameworkCore;

namespace Pivot.Framework.Infrastructure.Persistence.PostgreSQL.Locking;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Provides PostgreSQL advisory lock support for exclusive operations
///              such as outbox draining. Advisory locks are session-level, non-blocking,
///              and do not require any table or row to exist.
///
///              Uses pg_try_advisory_lock / pg_advisory_unlock for cooperative exclusion
///              across multiple application instances.
/// </summary>
public sealed class PostgreSqlAdvisoryLock : IAsyncDisposable
{
	private readonly DbContext _dbContext;
	private readonly long _lockId;
	private bool _acquired;

	private PostgreSqlAdvisoryLock(DbContext dbContext, long lockId)
	{
		_dbContext = dbContext;
		_lockId = lockId;
	}

	/// <summary>
	/// Attempts to acquire a PostgreSQL advisory lock. Returns null if the lock is already held.
	/// </summary>
	/// <param name="dbContext">The EF Core DbContext with an active connection.</param>
	/// <param name="lockId">A unique integer identifying the lock (e.g., hash of resource name).</param>
	/// <param name="cancellationToken">Token for cooperative cancellation.</param>
	/// <returns>An <see cref="PostgreSqlAdvisoryLock"/> if acquired, or null if already held.</returns>
	public static async Task<PostgreSqlAdvisoryLock?> TryAcquireAsync(
		DbContext dbContext,
		long lockId,
		CancellationToken cancellationToken = default)
	{
		var connection = dbContext.Database.GetDbConnection();
		if (connection.State != System.Data.ConnectionState.Open)
			await connection.OpenAsync(cancellationToken);

		await using var command = connection.CreateCommand();
		command.CommandText = $"SELECT pg_try_advisory_lock({lockId})";

		var result = await command.ExecuteScalarAsync(cancellationToken);
		var acquired = result is true or (object)"t";

		if (!acquired)
			return null;

		return new PostgreSqlAdvisoryLock(dbContext, lockId) { _acquired = true };
	}

	/// <summary>
	/// Computes a stable advisory lock ID from a resource name.
	/// Uses a simple hash to produce a 64-bit lock identifier.
	/// </summary>
	public static long ComputeLockId(string resourceName)
	{
		// Use a stable hash — not GetHashCode which varies between runs
		unchecked
		{
			long hash = 5381;
			foreach (var c in resourceName)
				hash = ((hash << 5) + hash) + c;
			return hash;
		}
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		if (!_acquired) return;

		try
		{
			var connection = _dbContext.Database.GetDbConnection();
			if (connection.State == System.Data.ConnectionState.Open)
			{
				await using var command = connection.CreateCommand();
				command.CommandText = $"SELECT pg_advisory_unlock({_lockId})";
				await command.ExecuteScalarAsync();
			}
		}
		catch
		{
			// Best-effort unlock — advisory locks are released on session disconnect anyway
		}

		_acquired = false;
	}
}
