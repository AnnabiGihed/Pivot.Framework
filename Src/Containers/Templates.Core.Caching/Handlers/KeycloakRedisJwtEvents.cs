using System.Text.Json;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Templates.Core.Caching.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Templates.Core.Caching.Handlers;

/// <summary>
/// Plugs Redis token caching and revocation checking into the Keycloak JWT bearer pipeline.
///
/// On <c>OnTokenValidated</c>:
///  1. Checks the revocation blacklist — rejects revoked tokens immediately.
///  2. Checks the claims cache — if found, replaces parsed claims with cached ones (fast path).
///  3. If not cached, extracts claims and stores them in Redis for next time.
///
/// Wire this up via <see cref="Extensions.RedisCachingExtensions.AddKeycloakRedisCache"/>.
/// </summary>
public sealed class KeycloakRedisJwtEvents : JwtBearerEvents
{
	#region Dependencies
	private readonly IDistributedTokenCache _tokenCache;
	private readonly ITokenRevocationCache _revocationCache;
	private readonly ILogger<KeycloakRedisJwtEvents> _logger;
	#endregion

	#region Constructor
	public KeycloakRedisJwtEvents(IDistributedTokenCache tokenCache, ITokenRevocationCache revocationCache, ILogger<KeycloakRedisJwtEvents> logger)
	{
		_logger = logger;
		_tokenCache = tokenCache;
		_revocationCache = revocationCache;
	}
	#endregion

	#region Overrides
	public override async Task TokenValidated(TokenValidatedContext context)
	{
		var accessToken = context.Request.Headers.Authorization
			.ToString()
			.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
			.Trim();

		if (string.IsNullOrEmpty(accessToken)) return;

		var jwt = context.SecurityToken as JwtSecurityToken;

		#region 1. Revocation check (individual token)
		if (await _revocationCache.IsRevokedAsync(accessToken, context.HttpContext.RequestAborted))
		{
			_logger.LogWarning("Rejected revoked token for sub={Sub}", jwt?.Subject ?? "unknown"); context.Fail("Token has been revoked.");
			return;
		}
		#endregion

		#region 2. Global user revocation check
		if (jwt is not null)
		{
			var iatClaim = jwt.Claims.FirstOrDefault(c => c.Type == "iat");

			if (iatClaim is not null && long.TryParse(iatClaim.Value, out var iatUnix))
			{
				var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix);
				if (await _revocationCache.IsIssuedBeforeRevocationAsync(jwt.Subject, issuedAt, context.HttpContext.RequestAborted))
				{
					_logger.LogWarning("Rejected globally-revoked token for user {Sub}", jwt.Subject);
					context.Fail("All tokens for this user have been revoked.");
					return;
				}
			}
		}
		#endregion

		#region 3. Claims cache fast path
		var cached = await _tokenCache.GetClaimsAsync(accessToken, context.HttpContext.RequestAborted);

		if (cached is not null)
		{
			// Replace the principal with the cached claims (skip re-parsing)
			var claims = RebuildClaims(cached);
			var identity = new ClaimsIdentity(claims, "keycloak", "preferred_username", ClaimTypes.Role);
			context.Principal = new ClaimsPrincipal(identity);
			_logger.LogDebug("Token claims served from Redis cache.");
			return;
		}
		#endregion

		#region 4. First request — extract and cache claims
		if (context.Principal?.Identity is ClaimsIdentity claimsIdentity && jwt is not null)
		{
			FlattenKeycloakRoles(claimsIdentity);

			var expiry = DateTimeOffset.FromUnixTimeSeconds(	long.Parse(jwt.Claims.First(c => c.Type == "exp").Value));

			var claimsToCache = new CachedTokenClaims
			{
				UserId = jwt.Subject,
				Email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
				Username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? string.Empty,
				Roles = claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
				AllClaims = jwt.Claims.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.First().Value),
			};

			await _tokenCache.SetClaimsAsync(accessToken, claimsToCache, expiry, context.HttpContext.RequestAborted);

			_logger.LogDebug("Token claims for user {UserId} written to Redis cache.", jwt.Subject);
		}
		#endregion
	}
	#endregion

	#region Helpers
	private static void FlattenKeycloakRoles(ClaimsIdentity identity)
	{
		var realmClaim = identity.FindFirst("realm_access");
		if (realmClaim is not null)
		{
			try
			{
				using var doc = JsonDocument.Parse(realmClaim.Value);
				if (doc.RootElement.TryGetProperty("roles", out var roles))
					foreach (var r in roles.EnumerateArray())
						if (r.GetString() is { } rv)
							identity.AddClaim(new Claim(ClaimTypes.Role, rv));
			}
			catch { /* malformed — skip */ }
		}

		var resourceClaim = identity.FindFirst("resource_access");
		if (resourceClaim is not null)
		{
			try
			{
				using var doc = JsonDocument.Parse(resourceClaim.Value);
				foreach (var client in doc.RootElement.EnumerateObject())
					if (client.Value.TryGetProperty("roles", out var roles))
						foreach (var r in roles.EnumerateArray())
							if (r.GetString() is { } rv)
								identity.AddClaim(new Claim(ClaimTypes.Role, rv));
			}
			catch { /* malformed — skip */ }
		}
	}

	private static List<Claim> RebuildClaims(CachedTokenClaims cached)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, cached.UserId),
			new("preferred_username",       cached.Username),
			new(ClaimTypes.Email,           cached.Email),
		};

		claims.AddRange(cached.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

		foreach (var (key, value) in cached.AllClaims)
			if (!claims.Exists(c => c.Type == key))
				claims.Add(new Claim(key, value));

		return claims;
	}
	#endregion
}