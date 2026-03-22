namespace Pivot.Framework.Authentication.Blazor.Storage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : The payload stored in Redis for a single authenticated Blazor session.
///              Contains the full Keycloak token set plus PKCE/OAuth2 flow state that
///              must survive the browser redirect round-trip to Keycloak.
/// </summary>
public sealed class BlazorTokenSession
{
	#region Token payload (populated after callback)
	/// <summary>
	/// The OpenID Connect ID token, if returned by Keycloak.
	/// </summary>
	public string? IdToken { get; set; }

	/// <summary>
	/// The OAuth2 access token used to authenticate API requests.
	/// </summary>
	public string? AccessToken { get; set; }

	/// <summary>
	/// The OAuth2 refresh token used to obtain new access tokens.
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// The UTC timestamp at which the access token expires.
	/// </summary>
	public DateTimeOffset? ExpiresAt { get; set; }

	/// <summary>
	/// The UTC timestamp at which the refresh token expires.
	/// </summary>
	public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
	#endregion

	#region Flow state (populated before redirect, cleared after callback)
	/// <summary>
	/// Nonce for ID-token replay protection.
	/// </summary>
	public string? Nonce { get; set; }
	/// <summary>
	/// Where to send the user after a successful login.
	/// </summary>
	public string? ReturnUrl { get; set; }

	/// <summary>
	/// OAuth2 state parameter for CSRF protection.
	/// </summary>
	public string? OAuthState { get; set; }

	/// <summary>
	/// PKCE code verifier stored server-side so it never touches the browser.
	/// </summary>
	public string? PkceVerifier { get; set; }
	#endregion

	#region Helpers
	/// <summary>
	/// Returns <c>true</c> when the session contains a valid access token and expiry.
	/// </summary>
	public bool HasTokens
	{
		get
		{
			return !string.IsNullOrEmpty(AccessToken) && ExpiresAt.HasValue;
		}
	}

	/// <summary>
	/// Returns <c>true</c> when the access token has expired (with a 30-second buffer).
	/// </summary>
	public bool IsExpired
	{
		get
		{
			return !HasTokens || DateTimeOffset.UtcNow >= ExpiresAt!.Value.AddSeconds(-30);
		}
	}

	/// <summary>
	/// Returns <c>true</c> when a refresh token is available and has not expired.
	/// </summary>
	public bool CanRefresh
	{
		get
		{
			if (string.IsNullOrEmpty(RefreshToken)) return false;
			if (RefreshTokenExpiresAt is null) return true;
			return DateTimeOffset.UtcNow < RefreshTokenExpiresAt.Value.AddSeconds(-30);
		}
	}
	#endregion
}