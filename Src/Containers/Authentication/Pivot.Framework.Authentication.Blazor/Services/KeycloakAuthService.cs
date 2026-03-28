using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Pivot.Framework.Authentication.Blazor.Storage;
using Pivot.Framework.Authentication.Events;
using Pivot.Framework.Authentication.Helpers;
using Pivot.Framework.Authentication.Responses;

namespace Pivot.Framework.Authentication.Blazor.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Full Keycloak authentication service for Blazor Server.
///
///              Security model — no tokens in the browser:
///              - PKCE code verifier, OAuth2 state, nonce and tokens are stored ONLY in Redis.
///              - The browser receives a single, opaque, HttpOnly session cookie.
///              - Login triggers a server-to-Keycloak redirect; the callback page calls
///                HandleCallbackAsync to complete the exchange.
///              - Token validation uses Keycloak's JWKS endpoint for full signature verification.
///              - Nonce is validated against the ID token to prevent replay attacks.
///              - ForceRefreshAsync enables safe 401 retry without looping.
/// </summary>
public sealed class KeycloakAuthService : IBlazorKeycloakAuthService
{
	#region Constants
	/// <summary>The name of the HttpOnly session cookie stored in the browser.</summary>
	internal const string SessionCookieName = "kc_session";
	/// <summary>Number of random bytes used to generate the opaque session ID.</summary>
	private const int SessionIdBytes = 32;
	#endregion

	#region Dependencies
	private readonly HttpClient _http;
	private readonly NavigationManager _nav;
	private readonly KeycloakOptions _options;
	private readonly ILogger<KeycloakAuthService> _logger;
	private readonly IBlazorTokenSessionStore _sessionStore;
	private readonly IHttpContextAccessor _httpContextAccessor;
	#endregion

	#region Circuit-scoped state (one instance per Blazor circuit)
	/// <summary>The opaque session ID stored in the browser's HttpOnly cookie.</summary>
	private string? _sessionId;
	/// <summary>The claims principal of the currently authenticated user.</summary>
	private ClaimsPrincipal? _user;
	/// <summary>The current Blazor token session holding the OAuth2 token set and flow state.</summary>
	private BlazorTokenSession? _session;
	/// <summary>
	/// Cached Keycloak OIDC configuration (signing keys).
	/// </summary>
	private OpenIdConnectConfiguration? _oidcConfig;
	/// <summary>Lock for thread-safe lazy loading of the OIDC configuration.</summary>
	private readonly SemaphoreSlim _oidcLock = new(1, 1);
	/// <summary>Lock for thread-safe token refresh operations.</summary>
	private readonly SemaphoreSlim _refreshLock = new(1, 1);
	#endregion

	#region Public API
	/// <inheritdoc />
	public ClaimsPrincipal? User => _user;
	/// <inheritdoc />
	public event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;
	/// <inheritdoc />
	public bool IsAuthenticated => _session is not null && _session.HasTokens && !_session.IsExpired;
	#endregion

	#region Constructor
	/// <summary>
	/// Initialises a new instance of <see cref="KeycloakAuthService"/> with the required dependencies.
	/// </summary>
	/// <param name="options">The Keycloak configuration options.</param>
	/// <param name="sessionStore">The Redis-backed session store for server-side token persistence.</param>
	/// <param name="httpClientFactory">The factory used to create the HTTP client for Keycloak token requests.</param>
	/// <param name="httpContextAccessor">The HTTP context accessor for reading and writing session cookies.</param>
	/// <param name="nav">The Blazor navigation manager used for redirects.</param>
	/// <param name="logger">The logger for diagnostic output.</param>
	public KeycloakAuthService(	IOptions<KeycloakOptions> options, IBlazorTokenSessionStore sessionStore, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, NavigationManager nav, ILogger<KeycloakAuthService> logger)
	{
		_nav = nav;
		_logger = logger;
		_options = options.Value;
		_options.Validate();
		_sessionStore = sessionStore;
		_httpContextAccessor = httpContextAccessor;
		_http = httpClientFactory.CreateClient(nameof(KeycloakAuthService));
	}
	#endregion

	#region Login — server-side PKCE, redirect to Keycloak
	/// <inheritdoc />
	public async Task<bool> LoginAsync(CancellationToken ct = default)
	{
		try
		{
			var sessionId = GenerateSessionId();
			var (codeVerifier, codeChallenge) = GeneratePkce();
			var state = GenerateRandom();
			var nonce = GenerateRandom();
			var returnUrl = _nav.Uri;

			var flowSession = new BlazorTokenSession
			{
				PkceVerifier = codeVerifier,
				OAuthState = state,
				Nonce = nonce,
				ReturnUrl = returnUrl
			};

			await _sessionStore.SaveAsync(sessionId, flowSession, ct);

			var ctx = _httpContextAccessor.HttpContext;
			if (ctx is not null && !ctx.Response.HasStarted)
			{
				SetSessionCookie(sessionId, persistent: false);
			}

			var stateWithSession = $"{state}.{sessionId}";

			var redirectUri = BuildCallbackUri();
			var authUrl = BuildAuthUrl(codeChallenge, stateWithSession, nonce, redirectUri);

			_nav.NavigateTo(authUrl, forceLoad: true);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Keycloak: failed to initiate login.");
			return false;
		}
	}
	#endregion

	#region Callback — exchange code, validate, persist
	/// <inheritdoc />
	public async Task<string?> HandleCallbackAsync(string code, string returnedState, CancellationToken ct = default)
	{
		try
		{
			var lastDot = returnedState.LastIndexOf('.');
			if (lastDot < 0)
			{
				_logger.LogWarning("Keycloak: malformed state — missing session ID segment.");
				return null;
			}

			var oauthState = returnedState[..lastDot];
			var sessionId = returnedState[(lastDot + 1)..];

			if (string.IsNullOrEmpty(sessionId))
			{
				_logger.LogWarning("Keycloak: session ID segment in state is empty.");
				return null;
			}

			var flowSession = await _sessionStore.GetAsync(sessionId, ct);
			if (flowSession is null)
			{
				_logger.LogWarning("Keycloak: callback session not found in Redis (expired or tampered).");
				return null;
			}

			if (string.IsNullOrEmpty(flowSession.OAuthState) || flowSession.OAuthState != oauthState)
			{
				_logger.LogWarning("Keycloak: OAuth2 state mismatch — possible CSRF attack.");
				await _sessionStore.RemoveAsync(sessionId, ct);
				return null;
			}

			if (string.IsNullOrEmpty(flowSession.PkceVerifier) || string.IsNullOrEmpty(flowSession.Nonce))
			{
				_logger.LogWarning("Keycloak: PKCE verifier or nonce missing from session.");
				await _sessionStore.RemoveAsync(sessionId, ct);
				return null;
			}

			// Capture ReturnUrl before clearing flow state below
			var returnUrl = flowSession.ReturnUrl ?? "/";

			var tokenResponse = await ExchangeCodeAsync(code, flowSession.PkceVerifier, BuildCallbackUri(), ct);

			if (tokenResponse.IdToken is not null)
				await ValidateIdTokenNonceAsync(tokenResponse.IdToken, flowSession.Nonce, ct);

			var principal = await ValidateAndParseAccessTokenAsync(tokenResponse.AccessToken, ct);

			var refreshExpiresAt = tokenResponse.RefreshExpiresIn > 0 ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.RefreshExpiresIn) : (DateTimeOffset?)null;

			flowSession.AccessToken = tokenResponse.AccessToken;
			flowSession.RefreshToken = tokenResponse.RefreshToken;
			flowSession.IdToken = tokenResponse.IdToken;
			flowSession.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
			flowSession.RefreshTokenExpiresAt = refreshExpiresAt;
			flowSession.PkceVerifier = null;
			flowSession.OAuthState = null;
			flowSession.Nonce = null;
			flowSession.ReturnUrl = null;

			await _sessionStore.SaveAsync(sessionId, flowSession, ct);

			SetSessionCookie(sessionId, persistent: true);

			_sessionId = sessionId;
			_session = flowSession;
			_user = principal;

			NotifyAuthStateChanged(isAuthenticated: true);

			_logger.LogInformation("Keycloak: user {Username} logged in via Blazor.", _user.FindFirst("preferred_username")?.Value);

			return returnUrl;
		}
		catch (SecurityTokenValidationException ex)
		{
			_logger.LogError(ex, "Keycloak: token validation failed during callback.");
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Keycloak: callback token exchange failed.");
			return null;
		}
	}
	#endregion

	#region Logout
	/// <inheritdoc />
	public async Task LogoutAsync(CancellationToken ct = default)
	{
		var current = _session;
		var sessionId = _sessionId;

		_session = null;
		_user = null;
		_sessionId = null;

		NotifyAuthStateChanged(isAuthenticated: false);

		// Remove from Redis
		if (sessionId is not null)
			await _sessionStore.RemoveAsync(sessionId, ct);

		// Revoke the refresh token server-side
		if (current?.RefreshToken is not null)
		{
			try { await RevokeTokenAsync(current.RefreshToken, ct); }
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Keycloak: refresh token revocation failed (ignored).");
			}
		}

		var ctx = _httpContextAccessor.HttpContext;
		if (ctx is not null && !ctx.Response.HasStarted)
		{
			DeleteSessionCookie();
		}

		if (current?.IdToken is not null)
		{
			var postLogoutUri = Uri.EscapeDataString(_nav.BaseUri.TrimEnd('/'));
			var endSessionUrl = $"{_options.LogoutUrl}" + $"?id_token_hint={Uri.EscapeDataString(current.IdToken)}" + $"&post_logout_redirect_uri={postLogoutUri}";

			_nav.NavigateTo(endSessionUrl, forceLoad: true);
		}
		else
		{
			_nav.NavigateTo("/", forceLoad: true);
		}

		_logger.LogInformation("Keycloak: user logged out.");
	}
	#endregion

	#region Get / force refresh access token
	/// <inheritdoc />
	public async Task<string> ForceRefreshAsync(CancellationToken ct = default)
	{
		if (_session?.CanRefresh != true)
			throw new UnauthorizedAccessException("No valid refresh token available.");

		return await RefreshInternalAsync(force: true, ct);
	}

	/// <inheritdoc />
	public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
	{
		if (_session is null || !_session.HasTokens)
			throw new UnauthorizedAccessException("Not logged in.");

		if (!_session.IsExpired)
			return _session.AccessToken!;

		if (!_session.CanRefresh)
			throw new UnauthorizedAccessException("Session expired. Please log in again.");

		return await RefreshInternalAsync(force: false, ct);
	}
	#endregion

	#region Restore session from cookie
	/// <summary>
	/// Attempts to restore an authenticated session from the HttpOnly session cookie.
	/// Refreshes the token if it is expired but the refresh token is still valid.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns><c>true</c> if the session was successfully restored; otherwise <c>false</c>.</returns>
	private async Task<bool> InitialiseFromSessionAsync(CancellationToken ct)
	{
		try
		{
			var sessionId = ReadSessionIdFromCookie();
			if (sessionId is null)
				return false;

			var session = await _sessionStore.GetAsync(sessionId, ct);
			if (session is null || !session.HasTokens)
				return false;

			if (session.CanRefresh)
			{
				try
				{
					_sessionId = sessionId;
					_session = session;
					await RefreshInternalAsync(force: true, ct);
					_logger.LogDebug("Keycloak: Blazor session validated and refreshed on restore.");
					return true;
				}
				catch (Exception ex)
				{
					_logger.LogInformation(ex, "Keycloak: session restore rejected by Keycloak (session terminated externally). Clearing local state.");

					_session = null;
					_user = null;
					_sessionId = null;

					await _sessionStore.RemoveAsync(sessionId, ct);

					var ctx = _httpContextAccessor.HttpContext;
					if (ctx is not null && !ctx.Response.HasStarted)
						DeleteSessionCookie();

					NotifyAuthStateChanged(isAuthenticated: false);
					return false;
				}
			}

			if (!session.IsExpired)
			{
				_sessionId = sessionId;
				_session = session;
				_user = await ValidateAndParseAccessTokenAsync(session.AccessToken!, ct);
				NotifyAuthStateChanged(isAuthenticated: true);
				_logger.LogDebug("Keycloak: Blazor session restored (no refresh token, trusting local expiry).");
				return true;
			}

			_logger.LogInformation("Keycloak: stored Blazor session expired and cannot be refreshed.");
			await _sessionStore.RemoveAsync(sessionId, ct);
			var httpCtx = _httpContextAccessor.HttpContext;
			if (httpCtx is not null && !httpCtx.Response.HasStarted)
				DeleteSessionCookie();
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Keycloak: failed to restore Blazor session.");
			return false;
		}
	}

	/// <inheritdoc />
	public async Task InitialiseFromCookieAsync(CancellationToken ct = default)
	{
		await InitialiseFromSessionAsync(ct);
	}

	/// <inheritdoc />
	public async Task<bool> TryRestoreSessionAsync(CancellationToken ct = default)
	{
		return await InitialiseFromSessionAsync(ct);
	}
	#endregion

	#region Internal refresh
	/// <summary>
	/// Fires the <see cref="AuthStateChanged"/> event with the current user and the given authentication flag.
	/// </summary>
	/// <param name="isAuthenticated">Whether the user is currently authenticated.</param>
	private void NotifyAuthStateChanged(bool isAuthenticated)
	{
		AuthStateChanged?.Invoke(
			this, new AuthStateChangedEventArgs(isAuthenticated, _user));
	}

	/// <summary>
	/// Performs a token refresh under a lock. When <paramref name="force"/> is <c>true</c>, refreshes
	/// unconditionally; otherwise skips the refresh if the current token is still valid.
	/// </summary>
	/// <param name="force">When <c>true</c>, forces the refresh regardless of token expiry.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>A fresh access token string.</returns>
	private async Task<string> RefreshInternalAsync(bool force, CancellationToken ct)
	{
		await _refreshLock.WaitAsync(ct);
		try
		{
			if (!force && _session is not null && !_session.IsExpired)
				return _session.AccessToken!;

			if (_session?.RefreshToken is null || !_session.CanRefresh)
				throw new UnauthorizedAccessException("No valid refresh token available.");

			var tokenResponse = await RefreshTokenGrantAsync(_session.RefreshToken, ct);

			var refreshExpiresAt = tokenResponse.RefreshExpiresIn > 0 ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.RefreshExpiresIn) : _session.RefreshTokenExpiresAt;

			_session.AccessToken = tokenResponse.AccessToken;
			_session.RefreshToken = tokenResponse.RefreshToken ?? _session.RefreshToken;
			_session.IdToken = tokenResponse.IdToken ?? _session.IdToken;
			_session.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
			_session.RefreshTokenExpiresAt = refreshExpiresAt;

			_user = await ValidateAndParseAccessTokenAsync(tokenResponse.AccessToken, ct);

			if (_sessionId is not null)
				await _sessionStore.SaveAsync(_sessionId, _session, ct);

			NotifyAuthStateChanged(isAuthenticated: true);
			return _session.AccessToken;
		}
		finally
		{
			_refreshLock.Release();
		}
	}
	#endregion

	#region Token exchange (Keycloak token endpoint)
	/// <summary>
	/// Calls Keycloak's token revocation endpoint to revoke the provided refresh token server-side.
	/// </summary>
	/// <param name="refreshToken">The refresh token to revoke.</param>
	/// <param name="ct">Cancellation token.</param>
	private async Task RevokeTokenAsync(string refreshToken, CancellationToken ct)
	{
		var body = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["token"] = refreshToken,
			["client_id"] = _options.ClientId,
			["token_type_hint"] = "refresh_token"
		});

		await _http.PostAsync(
			$"{_options.IssuerUrl}/protocol/openid-connect/revoke", body, ct);
	}
	/// <summary>
	/// Builds the refresh_token grant request and posts it to the Keycloak token endpoint.
	/// </summary>
	/// <param name="refreshToken">The refresh token to exchange for a new token set.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The new token response from Keycloak.</returns>
	private Task<KeycloakTokenResponse> RefreshTokenGrantAsync(string refreshToken, CancellationToken ct)
	{
		var body = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token",
			["refresh_token"] = refreshToken,
			["client_id"] = _options.ClientId
		});

		return PostToTokenEndpointAsync(body, ct);
	}
	/// <summary>
	/// Posts a form body to the Keycloak token endpoint and deserializes the JSON response.
	/// </summary>
	/// <param name="body">The URL-encoded form body for the token request.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The deserialized token response.</returns>
	private async Task<KeycloakTokenResponse> PostToTokenEndpointAsync(FormUrlEncodedContent body, CancellationToken ct)
	{
		var response = await _http.PostAsync(_options.TokenUrl, body, ct);

		if (!response.IsSuccessStatusCode)
		{
			var error = await response.Content.ReadAsStringAsync(ct);
			throw new InvalidOperationException($"Keycloak token endpoint returned {response.StatusCode}: {error}");
		}

		return await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: ct) ?? throw new InvalidOperationException(
				"Keycloak returned an empty token response.");
	}
	/// <summary>
	/// Builds the authorization_code grant request and posts it to the Keycloak token endpoint.
	/// </summary>
	/// <param name="code">The authorization code from the Keycloak callback.</param>
	/// <param name="codeVerifier">The PKCE code verifier matching the challenge sent at login.</param>
	/// <param name="redirectUri">The redirect URI that was used during the authorization request.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The token response from Keycloak.</returns>
	private Task<KeycloakTokenResponse> ExchangeCodeAsync(string code, string codeVerifier, string redirectUri, CancellationToken ct)
	{
		var body = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["code"] = code,
			["redirect_uri"] = redirectUri,
			["code_verifier"] = codeVerifier,
			["client_id"] = _options.ClientId,
			["grant_type"] = "authorization_code"
		});

		return PostToTokenEndpointAsync(body, ct);
	}
	#endregion

	#region Cryptographic JWT validation
	/// <summary>
	/// Lazily fetches Keycloak's OIDC discovery document (with JWKS signing keys) and caches it
	/// for the lifetime of the circuit. A real production deployment should add a background
	/// refresh for key rotation; for most use-cases Keycloak's keys are very long-lived.
	/// </summary>
	private async Task<OpenIdConnectConfiguration> GetOidcConfigurationAsync(CancellationToken ct)
	{
		if (_oidcConfig is not null)
			return _oidcConfig;

		await _oidcLock.WaitAsync(ct);
		try
		{
			if (_oidcConfig is not null)
				return _oidcConfig;

			var docRetriever = new HttpDocumentRetriever(_http)
			{
				RequireHttps = _options.RequireHttpsMetadata
			};

			_oidcConfig = await OpenIdConnectConfigurationRetriever.GetAsync(_options.MetadataUrl, docRetriever, ct);

			_logger.LogDebug("Keycloak OIDC configuration loaded. Keys: {Count}", _oidcConfig.SigningKeys.Count());
		}
		finally
		{
			_oidcLock.Release();
		}

		return _oidcConfig;
	}

	/// <summary>
	/// Validates the ID token's signature and checks the nonce claim for replay protection.
	/// </summary>
	private async Task ValidateIdTokenNonceAsync(string idToken, string expectedNonce, CancellationToken ct)
	{
		var config = await GetOidcConfigurationAsync(ct);

		var parameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = _options.IssuerUrl,
			ValidateAudience = true,
			ValidAudience = _options.ClientId,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromSeconds(30),
			IssuerSigningKeys = config.SigningKeys
		};

		var handler = new JsonWebTokenHandler();
		var result = await handler.ValidateTokenAsync(idToken, parameters);

		if (!result.IsValid)
		{
			_logger.LogWarning(result.Exception, "Keycloak: ID token signature validation failed.");
			throw new SecurityTokenValidationException("ID token failed cryptographic validation.", result.Exception);
		}

		if (!result.Claims.TryGetValue("nonce", out var actualNonce)
			|| actualNonce?.ToString() != expectedNonce)
		{
			_logger.LogWarning("Keycloak: ID token nonce mismatch — possible replay attack.");
			throw new SecurityTokenValidationException("ID token nonce validation failed.");
		}
	}

	/// <summary>
	/// Validates the access token's signature against Keycloak's JWKS, checks issuer,
	/// audience and lifetime. Returns a principal with flattened role claims.
	/// </summary>
	private async Task<ClaimsPrincipal> ValidateAndParseAccessTokenAsync(string accessToken, CancellationToken ct)
	{
		var config = await GetOidcConfigurationAsync(ct);

		var parameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = _options.IssuerUrl,
			ValidateAudience = !string.IsNullOrEmpty(_options.Audience),
			ValidAudience = _options.Audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromSeconds(30),
			IssuerSigningKeys = config.SigningKeys,
			NameClaimType = "preferred_username",
			RoleClaimType = ClaimTypes.Role
		};

		var handler = new JsonWebTokenHandler();
		var result = await handler.ValidateTokenAsync(accessToken, parameters);

		if (!result.IsValid)
		{
			_logger.LogError(result.Exception, "Keycloak: access token signature validation failed.");
			throw new SecurityTokenValidationException("Access token failed cryptographic validation.", result.Exception);
		}

		var claims = result.ClaimsIdentity.Claims.ToList();
		var identity = new ClaimsIdentity(claims, "keycloak", "preferred_username", ClaimTypes.Role);

		KeycloakRoleHelper.FlattenRoles(identity, _logger);

		return new ClaimsPrincipal(identity);
	}
	#endregion

	#region Cookie management
	/// <summary>
	/// Deletes the session cookie from the current HTTP response.
	/// </summary>
	private void DeleteSessionCookie()
	{
		var ctx = _httpContextAccessor.HttpContext;
		if (ctx is null) return;

		ctx.Response.Cookies.Delete(SessionCookieName);
	}
	/// <summary>
	/// Reads the session ID from the HttpOnly session cookie on the current HTTP request.
	/// Returns <c>null</c> if the cookie is absent.
	/// </summary>
	/// <returns>The session ID string, or <c>null</c> if not present.</returns>
	private string? ReadSessionIdFromCookie()
	{
		var ctx = _httpContextAccessor.HttpContext;
		return ctx?.Request.Cookies.TryGetValue(SessionCookieName, out var id) == true ? id : null;
	}
	/// <summary>
	/// Appends the session cookie to the current HTTP response.
	/// </summary>
	/// <param name="sessionId">The opaque session ID to store in the cookie.</param>
	/// <param name="persistent">When <c>true</c>, sets a 1-day expiry; otherwise the cookie is session-scoped.</param>
	private void SetSessionCookie(string sessionId, bool persistent)
	{
		var ctx = _httpContextAccessor.HttpContext;
		if (ctx is null) return;

		ctx.Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
		{
			Secure = true,
			HttpOnly = true,
			IsEssential = true,
			SameSite = SameSiteMode.Lax,
			Expires = persistent ? DateTimeOffset.UtcNow.AddDays(1) : null
		});
	}
	#endregion

	#region URL builders
	/// <summary>
	/// Builds the absolute callback URI used as the OAuth2 redirect_uri.
	/// </summary>
	/// <returns>The callback URI string.</returns>
	private string BuildCallbackUri()
	{
		return $"{_nav.BaseUri.TrimEnd('/')}/auth/callback";
	}
	/// <summary>
	/// Builds the Keycloak authorization endpoint URL with all required PKCE and OIDC parameters.
	/// </summary>
	/// <param name="codeChallenge">The PKCE code challenge (S256).</param>
	/// <param name="state">The combined OAuth2 state and session ID parameter.</param>
	/// <param name="nonce">The OIDC nonce for replay protection.</param>
	/// <param name="redirectUri">The OAuth2 redirect URI.</param>
	/// <returns>The full authorization URL to redirect the browser to.</returns>
	private string BuildAuthUrl(string codeChallenge, string state, string nonce, string redirectUri)
	{
		var scopes = Uri.EscapeDataString(_options.Scopes);
		return $"{_options.AuthorizationUrl}"
			 + $"?client_id={Uri.EscapeDataString(_options.ClientId)}"
			 + $"&response_type=code"
			 + $"&scope={scopes}"
			 + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
			 + $"&state={state}"
			 + $"&nonce={nonce}"
			 + $"&code_challenge={codeChallenge}"
			 + $"&code_challenge_method=S256";
	}
	#endregion

	#region Crypto helpers
	/// <summary>
	/// Generates a cryptographically random Base64Url-encoded string (16 bytes).
	/// </summary>
	/// <returns>A random URL-safe string suitable for use as OAuth2 state or OIDC nonce.</returns>
	private static string GenerateRandom()
	{
		return Base64UrlEncode(RandomNumberGenerator.GetBytes(16));
	}
	/// <summary>
	/// Generates a cryptographically random Base64Url-encoded session ID of <see cref="SessionIdBytes"/> bytes.
	/// </summary>
	/// <returns>A random session ID string used as the Redis key and cookie value.</returns>
	private static string GenerateSessionId()
	{
		return Base64UrlEncode(RandomNumberGenerator.GetBytes(SessionIdBytes));
	}
	/// <summary>
	/// Encodes a byte array as a URL-safe Base64 string with no padding characters.
	/// </summary>
	/// <param name="input">The byte array to encode.</param>
	/// <returns>A URL-safe Base64 string.</returns>
	private static string Base64UrlEncode(byte[] input)
	{
		return Convert.ToBase64String(input)
				.TrimEnd('=').Replace('+', '-').Replace('/', '_');
	}
	/// <summary>
	/// Generates a PKCE code verifier and its corresponding S256 code challenge.
	/// </summary>
	/// <returns>A tuple of (verifier, challenge) strings for the PKCE flow.</returns>
	private static (string verifier, string challenge) GeneratePkce()
	{
		var verifier = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
		var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
		var challenge = Base64UrlEncode(hash);
		return (verifier, challenge);
	}
	#endregion
}