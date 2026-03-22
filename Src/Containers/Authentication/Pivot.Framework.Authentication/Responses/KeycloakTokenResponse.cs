using System.Text.Json.Serialization;

namespace Pivot.Framework.Authentication.Responses;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Represents the JSON payload returned by Keycloak's token endpoint
///              for authorization_code and refresh_token grants.
///              Used to deserialize id_token, access_token, refresh_token and expiry metadata.
/// </summary>
public sealed class KeycloakTokenResponse
{
	#region Properties
	/// <summary>
	/// Number of seconds until the access token expires.
	/// </summary>
	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; init; }

	/// <summary>
	/// The OpenID Connect ID token, if returned by Keycloak.
	/// </summary>
	[JsonPropertyName("id_token")]
	public string? IdToken { get; init; }

	/// <summary>
	/// The OAuth2 refresh token used to obtain new access tokens.
	/// </summary>
	[JsonPropertyName("refresh_token")]
	public string? RefreshToken { get; init; }

	/// <summary>
	/// Seconds until the refresh token itself expires.
	/// Keycloak omits this field (or sets it to 0) for offline_access tokens that never expire.
	/// </summary>
	[JsonPropertyName("refresh_expires_in")]
	public int RefreshExpiresIn { get; init; }

	/// <summary>
	/// The OAuth2 access token used to authenticate API requests.
	/// </summary>
	[JsonPropertyName("access_token")]
	public string AccessToken { get; init; } = string.Empty;
	#endregion
}