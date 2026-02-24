namespace Templates.Core.Authentication.Blazor.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : The result of <see cref="IBlazorKeycloakAuthService.PrepareLoginAsync"/>.
///              Contains everything the caller needs to:
///              1. Write the session cookie (SessionId → kc_session)
///              2. Redirect the browser to Keycloak (AuthorizationUrl)
///
///              Separating data from side-effects lets consumers that run outside
///              of a live Blazor circuit (e.g. a minimal-API endpoint or a Razor Page)
///              handle the cookie write themselves, at a point where the HTTP response
///              has not yet been committed.
/// </summary>
public sealed class LoginFlowContext
{
	/// <summary>
	/// The opaque session identifier to store as the <c>kc_session</c> HttpOnly cookie.
	/// </summary>
	public required string SessionId { get; init; }

	/// <summary>
	/// The full Keycloak authorization URL including PKCE, state, nonce and redirect_uri.
	/// Redirect the browser here to begin the login.
	/// </summary>
	public required string AuthorizationUrl { get; init; }
}




