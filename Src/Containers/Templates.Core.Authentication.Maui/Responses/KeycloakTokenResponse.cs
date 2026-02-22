using System.Text.Json.Serialization;

namespace Templates.Core.Authentication.Maui.Responses;

public sealed class KeycloakTokenResponse
{
	[JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
	[JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
	[JsonPropertyName("id_token")] public string? IdToken { get; init; }
	[JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
}