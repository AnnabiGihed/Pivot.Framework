using System.Security.Claims;
using Templates.Core.Authentication.Maui.Events;

namespace Templates.Core.Authentication.Maui.Services;
/// <summary>
/// Contract for the Keycloak authentication service.
/// </summary>
public interface IKeycloakAuthService
{
	/// <summary>Observable auth state — fires whenever login/logout/refresh happens.</summary>
	event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;

	/// <summary>Whether the user is currently authenticated (has a valid access token).</summary>
	bool IsAuthenticated { get; }

	/// <summary>Claims from the current ID token / access token.</summary>
	ClaimsPrincipal? User { get; }

	/// <summary>
	/// Starts the browser-based Authorization Code + PKCE flow.
	/// Redirects to Keycloak login page and exchanges the code for tokens.
	/// </summary>
	Task<bool> LoginAsync(CancellationToken ct = default);

	/// <summary>
	/// Logs the user out: revokes the refresh token and clears local storage.
	/// Also opens the Keycloak end-session URL to log out of the SSO session.
	/// </summary>
	Task LogoutAsync(CancellationToken ct = default);

	/// <summary>
	/// Returns a valid access token, refreshing silently if needed.
	/// Throws <see cref="UnauthorizedAccessException"/> if not logged in.
	/// </summary>
	Task<string> GetAccessTokenAsync(CancellationToken ct = default);

	/// <summary>
	/// Tries to restore the session from SecureStorage on app start.
	/// Call this in your app shell before navigating.
	/// </summary>
	Task<bool> TryRestoreSessionAsync(CancellationToken ct = default);
}