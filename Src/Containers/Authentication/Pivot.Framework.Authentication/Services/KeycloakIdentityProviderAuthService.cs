using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Helpers;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed implementation of the provider-neutral auth service abstraction.
/// </summary>
public sealed class KeycloakIdentityProviderAuthService : IIdentityProviderAuthService
{
    #region Dependencies
    /// <summary>
    /// Shared HTTP client for communicating with Keycloak endpoints. Should be configured with appropriate timeouts and retry policies at the container level.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Configuration options for Keycloak integration, including client credentials and endpoint URLs. Validated on construction to ensure all required values are present and well-formed.
    /// </summary>
    private readonly KeycloakOptions _options;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="KeycloakIdentityProviderAuthService"/> class with the specified HTTP client and options.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public KeycloakIdentityProviderAuthService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
        _options.Validate();
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}
    #endregion

    #region IIdentityProviderAuthService Implementation
    /// <summary>
    /// Invalidates the user's session in Keycloak by sending a logout request to the configured logout endpoint with the provided refresh token and ID token hint.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task LogoutAsync(AuthLogoutRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var payload = new Dictionary<string, string?>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = request.RefreshToken,
            ["id_token_hint"] = request.IdTokenHint
        };

        using var response = await _httpClient.PostAsync(_options.LogoutUrl, BuildFormContent(payload), ct);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Retrieves the user's profile information from Keycloak's UserInfo endpoint using the provided access token.
	/// The method sends an authenticated GET request to the UserInfo URL and parses the JSON response to construct an IdentityProviderUser object containing relevant user details and claims.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IdentityProviderUser> GetUserProfileAsync(string accessToken, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = document.RootElement;
        return new IdentityProviderUser
        {
            Id = root.GetStringOrNull("sub") ?? string.Empty,
            Username = root.GetStringOrNull("preferred_username"),
            Email = root.GetStringOrNull("email"),
            FirstName = root.GetStringOrNull("given_name"),
            LastName = root.GetStringOrNull("family_name"),
            DisplayName = root.GetStringOrNull("name"),
            IsEnabled = true,
            Roles = [],
            Claims = root.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.String)
                .Select(property => new IdentityProviderClaim
                {
                    Type = property.Name,
                    Value = property.Value.GetString() ?? string.Empty
                })
                .ToArray()
        };
    }

    /// <summary>
    /// Uses the provided refresh token to obtain a new access token (and optionally a new refresh token) from Keycloak's token endpoint.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<AuthTokenResponse> RefreshTokenAsync(AuthRefreshTokenRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);

        var payload = new Dictionary<string, string?>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = request.RefreshToken
        };

        using var response = await _httpClient.PostAsync(_options.TokenUrl, BuildFormContent(payload), ct);
        response.EnsureSuccessStatusCode();

        return await ParseTokenResponseAsync(response, ct);
    }

    /// <summary>
    /// Builds the authorization URL to which users should be redirected to initiate the OAuth2 authorization code flow with PKCE.
	/// This method constructs the URL with all necessary query parameters based on the provided request and configured options.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<AuthAuthorizationResult> BuildAuthorizationUrlAsync(AuthAuthorizationRequest request, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.RedirectUri);

		var query = new Dictionary<string, string?>
		{
			["client_id"] = _options.ClientId,
			["redirect_uri"] = request.RedirectUri,
			["response_type"] = "code",
			["scope"] = string.IsNullOrWhiteSpace(request.Scope) ? _options.Scopes : request.Scope,
			["state"] = request.State,
			["code_challenge"] = request.CodeChallenge,
			["code_challenge_method"] = request.CodeChallenge is null ? null : request.CodeChallengeMethod,
			["login_hint"] = request.LoginHint,
			["prompt"] = request.Prompt
		};

		var queryString = string.Join("&", query
			.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
			.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));

		return Task.FromResult(new AuthAuthorizationResult
		{
			AuthorizationUrl = $"{_options.AuthorizationUrl}?{queryString}"
		});
	}

    /// <summary>
    /// Exchanges the authorization code received from Keycloak after the user completes the authorization flow for an access token (and optionally a refresh token and ID token).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
	public async Task<AuthTokenResponse> ExchangeAuthorizationCodeAsync(AuthCodeExchangeRequest request, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.RedirectUri);

		var payload = new Dictionary<string, string?>
		{
			["grant_type"] = "authorization_code",
			["client_id"] = _options.ClientId,
			["client_secret"] = _options.ClientSecret,
			["code"] = request.Code,
			["redirect_uri"] = request.RedirectUri,
			["code_verifier"] = request.CodeVerifier
		};

		using var response = await _httpClient.PostAsync(_options.TokenUrl, BuildFormContent(payload), ct);
		response.EnsureSuccessStatusCode();

		return await ParseTokenResponseAsync(response, ct);
	}
    #endregion

    #region Private Helpers
    /// <summary>
    /// Builds a FormUrlEncodedContent from a dictionary of key-value pairs, filtering out any entries with null or whitespace values.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private static FormUrlEncodedContent BuildFormContent(IReadOnlyDictionary<string, string?> values)
	{
		return new FormUrlEncodedContent(values
			.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
			.ToDictionary(pair => pair.Key, pair => pair.Value!));
	}

    /// <summary>
    /// Parses the token response from Keycloak, extracting relevant fields and calculating expiration times based on the current time and the "expires_in" values returned by Keycloak.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static async Task<AuthTokenResponse> ParseTokenResponseAsync(HttpResponseMessage response, CancellationToken ct)
	{
		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
		var root = document.RootElement;

		var expiresIn = root.TryGetProperty("expires_in", out var expiresInElement) ? expiresInElement.GetInt32() : 0;
		var refreshExpiresIn = root.TryGetProperty("refresh_expires_in", out var refreshExpiresElement) ? refreshExpiresElement.GetInt32() : 0;
		var now = DateTimeOffset.UtcNow;

		return new AuthTokenResponse
		{
			AccessToken = root.GetStringOrNull("access_token") ?? string.Empty,
			RefreshToken = root.GetStringOrNull("refresh_token"),
			IdToken = root.GetStringOrNull("id_token"),
			TokenType = root.GetStringOrNull("token_type") ?? "Bearer",
			Scope = root.GetStringOrNull("scope"),
			ExpiresAt = now.AddSeconds(expiresIn),
			RefreshTokenExpiresAt = refreshExpiresIn > 0 ? now.AddSeconds(refreshExpiresIn) : null
		};
	}
    #endregion
}
