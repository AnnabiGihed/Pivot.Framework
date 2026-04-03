using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Helpers;
using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Keycloak-backed implementation of the provider-neutral admin service abstraction.
/// </summary>
public sealed class KeycloakIdentityProviderAdminService : IIdentityProviderAdminService
{
	private readonly HttpClient _httpClient;
	private readonly KeycloakOptions _options;

	public KeycloakIdentityProviderAdminService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_options.Validate();
	}

	public async Task<IReadOnlyCollection<IdentityProviderUser>> GetUsersAsync(string? search = null, CancellationToken ct = default)
	{
		var url = string.IsNullOrWhiteSpace(search)
			? $"{_options.AdminBaseUrl}/users"
			: $"{_options.AdminBaseUrl}/users?search={Uri.EscapeDataString(search)}";

		using var request = await CreateAdminRequestAsync(HttpMethod.Get, url, ct);
		using var response = await _httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

		return document.RootElement.EnumerateArray()
			.Select(MapUser)
			.ToArray();
	}

	public async Task<IdentityProviderUser?> GetUserByIdAsync(string userId, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userId);

		using var request = await CreateAdminRequestAsync(HttpMethod.Get, $"{_options.AdminBaseUrl}/users/{Uri.EscapeDataString(userId)}", ct);
		using var response = await _httpClient.SendAsync(request, ct);

		if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			return null;

		response.EnsureSuccessStatusCode();
		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
		return MapUser(document.RootElement);
	}

	public async Task<IdentityProviderUser?> GetUserByEmailAsync(string email, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(email);

		using var request = await CreateAdminRequestAsync(HttpMethod.Get, $"{_options.AdminBaseUrl}/users?email={Uri.EscapeDataString(email)}&exact=true", ct);
		using var response = await _httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
		return document.RootElement.EnumerateArray()
			.Select(MapUser)
			.FirstOrDefault();
	}

	public async Task<string> CreateUserAsync(CreateIdentityProviderUserRequest request, CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Username);

		using var httpRequest = await CreateAdminRequestAsync(HttpMethod.Post, $"{_options.AdminBaseUrl}/users", ct);
		httpRequest.Content = JsonContent.Create(new
		{
			username = request.Username,
			email = request.Email,
			firstName = request.FirstName,
			lastName = request.LastName,
			enabled = request.IsEnabled
		});

		using var response = await _httpClient.SendAsync(httpRequest, ct);
		response.EnsureSuccessStatusCode();

		var userId = ExtractCreatedResourceId(response);
		if (request.Roles.Count > 0)
			await AssignRolesAsync(userId, request.Roles, ct);

		return userId;
	}

	public async Task UpdateUserAsync(string userId, UpdateIdentityProviderUserRequest request, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userId);
		ArgumentNullException.ThrowIfNull(request);

		using var httpRequest = await CreateAdminRequestAsync(HttpMethod.Put, $"{_options.AdminBaseUrl}/users/{Uri.EscapeDataString(userId)}", ct);
		httpRequest.Content = JsonContent.Create(new
		{
			username = request.Username,
			email = request.Email,
			firstName = request.FirstName,
			lastName = request.LastName,
			enabled = request.IsEnabled
		});

		using var response = await _httpClient.SendAsync(httpRequest, ct);
		response.EnsureSuccessStatusCode();
	}

	public async Task AssignRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userId);
		ArgumentNullException.ThrowIfNull(roles);

		if (roles.Count == 0)
			return;

		var roleRepresentations = await GetRoleRepresentationsAsync(roles, ct);

		using var request = await CreateAdminRequestAsync(HttpMethod.Post, $"{_options.AdminBaseUrl}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm", ct);
		request.Content = JsonContent.Create(roleRepresentations);

		using var response = await _httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();
	}

	public async Task RemoveRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userId);
		ArgumentNullException.ThrowIfNull(roles);

		if (roles.Count == 0)
			return;

		var roleRepresentations = await GetRoleRepresentationsAsync(roles, ct);

		using var request = await CreateAdminRequestAsync(HttpMethod.Delete, $"{_options.AdminBaseUrl}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm", ct);
		request.Content = JsonContent.Create(roleRepresentations);

		using var response = await _httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();
	}

	private async Task<HttpRequestMessage> CreateAdminRequestAsync(HttpMethod method, string url, CancellationToken ct)
	{
		var accessToken = await GetAdminAccessTokenAsync(ct);
		var request = new HttpRequestMessage(method, url);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		return request;
	}

	private async Task<string> GetAdminAccessTokenAsync(CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(_options.EffectiveAdminClientSecret))
			throw new InvalidOperationException("Keycloak admin operations require a client secret. Configure Keycloak.AdminClientSecret or Keycloak.ClientSecret.");

		var payload = new Dictionary<string, string>
		{
			["grant_type"] = "client_credentials",
			["client_id"] = _options.EffectiveAdminClientId,
			["client_secret"] = _options.EffectiveAdminClientSecret!
		};

		using var response = await _httpClient.PostAsync(_options.TokenUrl, new FormUrlEncodedContent(payload), ct);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(ct);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
		return document.RootElement.GetStringOrNull("access_token")
			?? throw new InvalidOperationException("Keycloak admin token response did not contain an access_token.");
	}

	private async Task<IReadOnlyCollection<object>> GetRoleRepresentationsAsync(IReadOnlyCollection<string> roles, CancellationToken ct)
	{
		var representations = new List<object>(roles.Count);

		foreach (var role in roles)
		{
			using var request = await CreateAdminRequestAsync(HttpMethod.Get, $"{_options.AdminBaseUrl}/roles/{Uri.EscapeDataString(role)}", ct);
			using var response = await _httpClient.SendAsync(request, ct);
			response.EnsureSuccessStatusCode();

			await using var stream = await response.Content.ReadAsStreamAsync(ct);
			using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
			var root = document.RootElement;

			representations.Add(new
			{
				id = root.GetStringOrNull("id"),
				name = root.GetStringOrNull("name"),
				description = root.GetStringOrNull("description")
			});
		}

		return representations;
	}

	private static string ExtractCreatedResourceId(HttpResponseMessage response)
	{
		var location = response.Headers.Location?.ToString();
		if (string.IsNullOrWhiteSpace(location))
			throw new InvalidOperationException("Keycloak create-user response did not include a Location header.");

		return location.TrimEnd('/').Split('/').Last();
	}

	private static IdentityProviderUser MapUser(JsonElement element)
	{
		var claims = new List<IdentityProviderClaim>();

		if (element.TryGetProperty("attributes", out var attributes) && attributes.ValueKind == JsonValueKind.Object)
		{
			foreach (var attribute in attributes.EnumerateObject())
			{
				if (attribute.Value.ValueKind != JsonValueKind.Array)
					continue;

				foreach (var value in attribute.Value.EnumerateArray())
				{
					if (value.ValueKind != JsonValueKind.String)
						continue;

					claims.Add(new IdentityProviderClaim
					{
						Type = attribute.Name,
						Value = value.GetString() ?? string.Empty
					});
				}
			}
		}

		return new IdentityProviderUser
		{
			Id = element.GetStringOrNull("id") ?? string.Empty,
			Username = element.GetStringOrNull("username"),
			Email = element.GetStringOrNull("email"),
			FirstName = element.GetStringOrNull("firstName"),
			LastName = element.GetStringOrNull("lastName"),
			DisplayName = string.Join(' ', new[] { element.GetStringOrNull("firstName"), element.GetStringOrNull("lastName") }.Where(part => !string.IsNullOrWhiteSpace(part))),
			IsEnabled = element.GetBooleanOrDefault("enabled", true),
			Roles = [],
			Claims = claims
		};
	}
}
