using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
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
    #region Dependencies
    /// <summary>
    /// HttpClient instance configured for Keycloak admin API access. Must be registered in DI with appropriate base address and timeouts.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Keycloak configuration options, including admin client credentials and endpoint URLs. Must be registered in DI and validated on construction.
    /// </summary>
    private readonly KeycloakOptions _options;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes the service with required dependencies. Validates configuration options to ensure admin operations can be performed.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public KeycloakIdentityProviderAdminService(HttpClient httpClient, IOptions<KeycloakOptions> options)
	{
        _options.Validate();
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}
    #endregion

    #region IIdentityProviderAdminService Implementation
    /// <summary>
    /// Retrieves a user by their unique identifier. Returns null if no user with the specified ID exists. Maps Keycloak's user representation to the provider-neutral IdentityProviderUser model.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Retrieves a user by their email address. Returns null if no user with the specified email exists. Uses Keycloak's search API to find users by email and maps the first result to the provider-neutral IdentityProviderUser model.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a new user in Keycloak based on the provided request data. Returns the unique identifier of the newly created user. If roles are specified in the request, they will be assigned to the user after creation. Maps the provider-neutral CreateIdentityProviderUserRequest model to Keycloak's user representation for the API request. Handles response parsing to extract the created user's ID from the Location header. Throws exceptions for any API errors encountered during the process.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Assigns the specified roles to the user with the given identifier. Roles are specified by their names and must already exist in Keycloak. The method retrieves the full role representations for the specified role names and then makes an API call to assign those roles to the user. Throws exceptions if the user does not exist, if any of the specified roles do not exist, or if any API errors occur during the process.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roles"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Removes the specified roles from the user with the given identifier. Roles are specified by their names and must already exist in Keycloak. The method retrieves the full role representations for the specified role names and then makes an API call to remove those roles from the user. Throws exceptions if the user does not exist, if any of the specified roles do not exist, or if any API errors occur during the process.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roles"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Updates the properties of an existing user in Keycloak based on the provided request data. The user to update is identified by their unique identifier. The method maps the provider-neutral UpdateIdentityProviderUserRequest model to Keycloak's user representation for the API request. Only the properties that are not null in the request will be updated; properties that are null will be left unchanged. Throws exceptions if the user does not exist or if any API errors occur during the process.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Retrieves a list of users from Keycloak, optionally filtered by a search term. If a search term is provided, it will be used to filter users by username, email, or other attributes according to Keycloak's search capabilities. The method maps the Keycloak user representations to the provider-neutral IdentityProviderUser model for each user in the result set. Throws exceptions if any API errors occur during the process.
    /// </summary>
    /// <param name="search"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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
    #endregion

    #region Private Helper Methods
    /// <summary>
    /// Maps a JsonElement representing a Keycloak user to the provider-neutral IdentityProviderUser model. Extracts standard user properties such as username, email, first name, last name, and enabled status. Also processes the "attributes" property of the Keycloak user representation to extract custom claims and map them to IdentityProviderClaim instances. The method assumes that the input JsonElement is a valid Keycloak user representation and does not perform extensive validation on the presence of expected properties.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Obtains an access token for Keycloak admin API operations using the client credentials grant. The method constructs a form-urlencoded request with the necessary parameters (grant_type, client_id, client_secret) and sends it to the configured token endpoint. It then parses the response to extract the access token string. This token is used in the Authorization header of subsequent admin API requests. Throws exceptions if the client secret is not configured or if the token request fails for any reason (e.g., invalid credentials, network issues).
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Extracts the unique identifier of a newly created resource (e.g., user) from the Location header of a Keycloak API response. When a resource is successfully created via a POST request, Keycloak returns a 201 Created status code along with a Location header that contains the URL of the newly created resource. This method parses that URL to extract the last segment, which is typically the unique identifier of the resource (e.g., user ID). Throws an exception if the Location header is missing or does not contain a valid URL.
    /// </summary>
    /// <param name="response">The HTTP response message from the Keycloak API.</param>
    /// <returns>The unique identifier of the newly created resource.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Location header is missing or invalid.</exception>
    private static string ExtractCreatedResourceId(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrWhiteSpace(location))
            throw new InvalidOperationException("Keycloak create-user response did not include a Location header.");

        return location.TrimEnd('/').Split('/').Last();
    }

    /// <summary>
    /// Creates an HttpRequestMessage for Keycloak admin API calls, including the necessary Authorization header with a bearer token. The method first obtains an access token using the client credentials grant and then constructs an HttpRequestMessage with the specified HTTP method and URL. The access token is included in the Authorization header as a Bearer token. This helper method centralizes the logic for authenticating admin API requests and ensures that all such requests include valid credentials. Throws exceptions if obtaining the access token fails.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<HttpRequestMessage> CreateAdminRequestAsync(HttpMethod method, string url, CancellationToken ct)
	{
		var accessToken = await GetAdminAccessTokenAsync(ct);
		var request = new HttpRequestMessage(method, url);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		return request;
	}

    /// <summary>
    /// Retrieves the full role representations for a given list of role names. Keycloak's API for assigning roles to users requires the full role representation (including ID, name, and description) rather than just the role name. This method iterates over the provided role names, makes individual API calls to retrieve each role's details, and constructs a list of anonymous objects containing the required properties (id, name, description) for each role. Throws exceptions if any of the specified roles do not exist or if any API errors occur during the retrieval process.
    /// </summary>
    /// <param name="roles">The list of role names to retrieve representations for.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only collection of role representations.</returns>
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
    #endregion
}