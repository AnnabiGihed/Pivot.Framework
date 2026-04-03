using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Storage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral session persistence abstraction for backend auth APIs.
/// </summary>
public interface IAuthSessionStore
{
	/// <summary>
	/// Gets an auth session by identifier.
	/// </summary>
	Task<AuthSession?> GetAsync(string sessionId, CancellationToken ct = default);

	/// <summary>
	/// Saves or replaces an auth session.
	/// </summary>
	Task SaveAsync(AuthSession session, CancellationToken ct = default);

	/// <summary>
	/// Removes an auth session.
	/// </summary>
	Task RemoveAsync(string sessionId, CancellationToken ct = default);
}
