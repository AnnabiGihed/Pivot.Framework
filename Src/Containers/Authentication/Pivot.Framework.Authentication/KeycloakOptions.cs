namespace Pivot.Framework.Authentication;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Central Keycloak configuration options.
///              Bind this from appsettings.json under the section "Keycloak".
/// </summary>
public sealed class KeycloakOptions
{
	#region Constants
	/// <summary>
	/// The appsettings.json section name used to bind this options class.
	/// </summary>
	public const string SectionName = "Keycloak";
	#endregion

	#region Properties
	/// <summary>
	/// OAuth2 client_secret. Leave empty for public (PKCE) clients.
	/// </summary>
	public string? ClientSecret { get; set; }

	/// <summary>
	/// Keycloak realm name.
	/// </summary>
	public string Realm { get; set; } = string.Empty;

	/// <summary>
	/// Base URL of the Keycloak server. E.g. https://auth.example.com
	/// </summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>
	/// OAuth2 client_id for the application.
	/// </summary>
	public string ClientId { get; set; } = string.Empty;

	/// <summary>
	/// Optional dedicated client_id used for admin and machine-to-machine operations.
	/// Falls back to <see cref="ClientId"/> when omitted.
	/// </summary>
	public string? AdminClientId { get; set; }

	/// <summary>
	/// Optional dedicated client_secret used for admin and machine-to-machine operations.
	/// Falls back to <see cref="ClientSecret"/> when omitted.
	/// </summary>
	public string? AdminClientSecret { get; set; }

	/// <summary>
	/// Keycloak audience claim. Often same as ClientId, or a dedicated API audience.
	/// </summary>
	public string Audience { get; set; } = string.Empty;

	/// <summary>
	/// Whether to require HTTPS metadata. Set false only in development.
	/// </summary>
	public bool RequireHttpsMetadata { get; set; } = true;

	/// <summary>
	/// Scopes to request. Defaults to "openid profile email offline_access".
	/// </summary>
	public string Scopes { get; set; } = "openid profile email offline_access";
	#endregion

	#region Computed helpers
	/// <summary>
	/// Issuer URL for this realm.
	/// </summary>
	public string IssuerUrl => $"{BaseUrl.TrimEnd('/')}/realms/{Realm}";

	/// <summary>
	/// Token endpoint.
	/// </summary>
	public string TokenUrl => $"{IssuerUrl}/protocol/openid-connect/token";

	/// <summary>
	/// Logout endpoint.
	/// </summary>
	public string LogoutUrl => $"{IssuerUrl}/protocol/openid-connect/logout";

	/// <summary>
	/// OIDC well-known metadata URL.
	/// </summary>
	public string MetadataUrl => $"{IssuerUrl}/.well-known/openid-configuration";

	/// <summary>
	/// Authorization endpoint (used by Swagger UI and MAUI).
	/// </summary>
	public string AuthorizationUrl => $"{IssuerUrl}/protocol/openid-connect/auth";

	/// <summary>
	/// UserInfo endpoint for resolving the authenticated user's profile.
	/// </summary>
	public string UserInfoUrl => $"{IssuerUrl}/protocol/openid-connect/userinfo";

	/// <summary>
	/// Token introspection endpoint.
	/// </summary>
	public string IntrospectionUrl => $"{IssuerUrl}/protocol/openid-connect/token/introspect";

	/// <summary>
	/// Token revocation endpoint.
	/// </summary>
	public string RevocationUrl => $"{IssuerUrl}/protocol/openid-connect/revoke";

	/// <summary>
	/// Realm administration API base URL.
	/// </summary>
	public string AdminBaseUrl => $"{BaseUrl.TrimEnd('/')}/admin/realms/{Realm}";

	/// <summary>
	/// Effective administrative client identifier.
	/// </summary>
	public string EffectiveAdminClientId => string.IsNullOrWhiteSpace(AdminClientId) ? ClientId : AdminClientId;

	/// <summary>
	/// Effective administrative client secret.
	/// </summary>
	public string? EffectiveAdminClientSecret => string.IsNullOrWhiteSpace(AdminClientSecret) ? ClientSecret : AdminClientSecret;

	/// <summary>
	/// Validates that the required settings are present.
	/// </summary>
	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(BaseUrl))
			throw new InvalidOperationException($"{SectionName}.{nameof(BaseUrl)} is required.");
		if (string.IsNullOrWhiteSpace(Realm))
			throw new InvalidOperationException($"{SectionName}.{nameof(Realm)} is required.");
		if (string.IsNullOrWhiteSpace(ClientId))
			throw new InvalidOperationException($"{SectionName}.{nameof(ClientId)} is required.");
	}
	#endregion
}
