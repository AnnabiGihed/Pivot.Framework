namespace Templates.Core.Authentication.Maui;

/// <summary>
/// Represents the OAuth2 token set returned by Keycloak.
/// Immutable — create a new instance on every refresh.
/// </summary>
public sealed record KeycloakTokenSet
{
	public string AccessToken { get; init; } = string.Empty;
	public string? RefreshToken { get; init; }
	public string? IdToken { get; init; }
	public DateTimeOffset ExpiresAt { get; init; }

	/// <summary>Returns true when the access token has expired (with 30s buffer).</summary>
	public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt.AddSeconds(-30);

	/// <summary>Returns true when we have a refresh token available.</summary>
	public bool CanRefresh => !string.IsNullOrEmpty(RefreshToken);
}