using System.Text.Json;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Helpers;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed token introspection implementation.
/// </summary>
public sealed class KeycloakTokenIntrospectionService : ITokenIntrospectionService
{
    #region Dependencies
    /// <summary>
    /// HTTP client configured for Keycloak interactions, injected via DI.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Keycloak options containing necessary configuration for token introspection, injected via DI.
    /// </summary>
    private readonly KeycloakOptions _options;
	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of <see cref="KeycloakTokenIntrospectionService"/>.
	/// </summary>
	/// <param name="httpClient">The HTTP client configured for Keycloak.</param>
	/// <param name="options">The Keycloak options.</param>
	public KeycloakTokenIntrospectionService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_options.Validate();
	}
	#endregion

	#region ITokenIntrospectionService Implementation
	/// <summary>
	/// Introspects the supplied token through Keycloak.
	/// </summary>
	public async Task<TokenIntrospectionResult> IntrospectTokenAsync(string token, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(token);

		var payload = new Dictionary<string, string?>
		{
			["client_id"] = _options.EffectiveAdminClientId,
			["client_secret"] = _options.EffectiveAdminClientSecret,
			["token"] = token
		};

		using var response = await _httpClient.PostAsync(_options.IntrospectionUrl, new FormUrlEncodedContent(payload
			.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
			.ToDictionary(pair => pair.Key, pair => pair.Value!)), ct);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
		var root = document.RootElement;
		var scopes = (root.GetStringOrNull("scope") ?? string.Empty)
			.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		return new TokenIntrospectionResult
		{
			IsActive = root.GetBooleanOrDefault("active"),
			SubjectId = root.GetStringOrNull("sub"),
			Username = root.GetStringOrNull("username"),
			ClientId = root.GetStringOrNull("client_id"),
			ExpiresAt = root.TryGetProperty("exp", out var expElement) && expElement.ValueKind == JsonValueKind.Number
				? DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64())
				: null,
			Scopes = scopes,
			Roles = []
		};
	}
	#endregion
}