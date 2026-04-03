using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Helpers;
using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed implementation of the provider-neutral auth service abstraction.
/// </summary>
public sealed class KeycloakIdentityProviderAuthService : IIdentityProviderAuthService
{
	private readonly HttpClient _httpClient;
	private readonly KeycloakOptions _options;

	public KeycloakIdentityProviderAuthService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_options.Validate();
	}

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

	private static FormUrlEncodedContent BuildFormContent(IReadOnlyDictionary<string, string?> values)
	{
		return new FormUrlEncodedContent(values
			.Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
			.ToDictionary(pair => pair.Key, pair => pair.Value!));
	}

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
}
