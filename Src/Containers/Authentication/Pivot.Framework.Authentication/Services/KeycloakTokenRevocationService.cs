using Microsoft.Extensions.Options;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed token revocation implementation.
/// </summary>
public sealed class KeycloakTokenRevocationService : ITokenRevocationService
{
	private readonly HttpClient _httpClient;
	private readonly KeycloakOptions _options;

	public KeycloakTokenRevocationService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_options.Validate();
	}

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
}
