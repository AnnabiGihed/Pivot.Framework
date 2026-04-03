using System.Collections.Concurrent;
using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Storage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Simple in-memory implementation of <see cref="IAuthSessionStore"/> intended
///              for lightweight services, local development, and automated tests.
/// </summary>
public sealed class InMemoryAuthSessionStore : IAuthSessionStore
{
	private readonly ConcurrentDictionary<string, AuthSession> _sessions = new(StringComparer.Ordinal);

	/// <inheritdoc />
	public Task<AuthSession?> GetAsync(string sessionId, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

		_sessions.TryGetValue(sessionId, out var session);
		return Task.FromResult(session);
	}

	/// <inheritdoc />
	public Task SaveAsync(AuthSession session, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(session);
		ArgumentException.ThrowIfNullOrWhiteSpace(session.SessionId);

		_sessions[session.SessionId] = session;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task RemoveAsync(string sessionId, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

		_sessions.TryRemove(sessionId, out _);
		return Task.CompletedTask;
	}
}
