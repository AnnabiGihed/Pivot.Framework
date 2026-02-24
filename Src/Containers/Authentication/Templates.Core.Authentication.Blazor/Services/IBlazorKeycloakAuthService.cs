using Templates.Core.Authentication.Services;

namespace Templates.Core.Authentication.Blazor.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Extends <see cref="IKeycloakAuthService"/> with Blazor-specific operations
///              needed for the redirect-based PKCE flow and server-side session management.
/// </summary>
public interface IBlazorKeycloakAuthService : IKeycloakAuthService
{
	/// <summary>
	/// Initialises the service from the session cookie present on the current request.
	/// Call this early in the Blazor circuit lifecycle (e.g. in a layout's OnInitializedAsync).
	/// </summary>
	Task InitialiseFromCookieAsync(CancellationToken ct = default);

	/// <summary>
	/// Exchanges the authorization code returned by Keycloak at the /auth/callback page.
	/// </summary>
	/// <param name="code">The authorization code from the query string.</param>
	/// <param name="returnedState">The state parameter from the query string (for CSRF validation).</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns><c>true</c> on success; <c>false</c> on state mismatch, nonce failure, or exchange error.</returns>
	Task<bool> HandleCallbackAsync(string code, string returnedState, CancellationToken ct = default);

	/// <summary>
	/// Prepares the PKCE login flow and returns the data needed to complete it,
	/// WITHOUT writing any cookies or performing any redirect.
	///
	/// Use this instead of <see cref="IKeycloakAuthService.LoginAsync"/> when inside
	/// an interactive Blazor Server circuit, where the HTTP response is already committed
	/// and Set-Cookie headers can no longer be written.
	///
	/// The caller is responsible for:
	/// 1. Writing <see cref="LoginFlowContext.SessionId"/> as an HttpOnly cookie named
	///    <c>kc_session</c> on the outgoing response.
	/// 2. Redirecting the browser to <see cref="LoginFlowContext.AuthorizationUrl"/>.
	///
	/// Both steps must happen in a real HTTP request context (e.g. a minimal-API endpoint
	/// mapped at <c>/auth/initiate-login</c>) rather than inside a Blazor event handler.
	/// </summary>
	Task<LoginFlowContext> PrepareLoginAsync(string? returnUrl = null, CancellationToken ct = default);
}