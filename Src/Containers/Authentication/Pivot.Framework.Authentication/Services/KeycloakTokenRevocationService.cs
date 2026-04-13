using Microsoft.Extensions.Options;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed token revocation implementation.
/// </summary>
public sealed class KeycloakTokenRevocationService : ITokenRevocationService
{
    #region Dependencies
    /// <summary>
    /// HTTP client configured for Keycloak interactions, injected via DI.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Keycloak options containing necessary configuration for token revocation, injected via DI.
    /// </summary>
    private readonly KeycloakOptions _options;
	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of <see cref="KeycloakTokenRevocationService"/>.
	/// </summary>
	/// <param name="httpClient">The HTTP client configured for Keycloak.</param>
	/// <param name="options">The Keycloak options.</param>
	public KeycloakTokenRevocationService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_options.Validate();
	}
	#endregion

	#region ITokenRevocationService Implementation
	/// <summary>
	/// Revokes the supplied token through Keycloak.
	/// </summary>
	public async Task RevokeTokenAsync(string token, string? tokenTypeHint = null, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(token);

		var payload = new Dictionary<string, string?>
		{
			["client_id"] = _options.EffectiveAdminClientId,
			["client_secret"] = _options.EffectiveAdminClientSecret,
			["token"] = token,
			["token_type_hint"] = tokenTypeHint
		};

		using var response = await _httpClient.PostAsync(_options.RevocationUrl, new FormUrlEncodedContent(payload
			.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
			.ToDictionary(pair => pair.Key, pair => pair.Value!)), ct);
		response.EnsureSuccessStatusCode();
	}
	#endregion
}