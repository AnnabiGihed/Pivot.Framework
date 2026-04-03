namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral token response returned by backend identity-provider services.
/// </summary>
public sealed class AuthTokenResponse
{
	/// <summary>
	/// Access token for API calls.
	/// </summary>
	public string AccessToken { get; set; } = string.Empty;

	/// <summary>
	/// Refresh token when issued by the provider.
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// ID token when issued by the provider.
	/// </summary>
	public string? IdToken { get; set; }

	/// <summary>
	/// Token type. Defaults to Bearer.
	/// </summary>
	public string TokenType { get; set; } = "Bearer";

	/// <summary>
	/// Scope string returned by the provider.
	/// </summary>
	public string? Scope { get; set; }

	/// <summary>
	/// Access token expiry timestamp.
	/// </summary>
	public DateTimeOffset ExpiresAt { get; set; }

	/// <summary>
	/// Optional refresh token expiry timestamp.
	/// </summary>
	public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
}
