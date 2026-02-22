using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Templates.Core.Authentication.Helpers;

/// <summary>
/// Transforms Keycloak-specific JWT claims into standard .NET role claims.
///
/// Keycloak places roles in two locations:
///   - realm_access.roles            → realm-level roles (e.g. "admin")
///   - resource_access.{clientId}.roles → client-level roles (e.g. "read:routes")
///
/// This transformer copies both into ClaimTypes.Role so that
/// [Authorize(Roles = "admin")] works out of the box.
/// </summary>
internal static class KeycloakClaimsTransformer
{
	public static void FlattenRoles(TokenValidatedContext ctx)
	{
		if (ctx.Principal?.Identity is not ClaimsIdentity identity)
			return;

		// ── Realm roles ──────────────────────────────────────────────────────────
		var realmAccessClaim = identity.FindFirst("realm_access");
		if (realmAccessClaim is not null)
		{
			try
			{
				using var doc = JsonDocument.Parse(realmAccessClaim.Value);
				if (doc.RootElement.TryGetProperty("roles", out var roles))
				{
					foreach (var role in roles.EnumerateArray())
					{
						var roleValue = role.GetString();
						if (!string.IsNullOrEmpty(roleValue))
							identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
					}
				}
			}
			catch (JsonException) { /* malformed claim — skip */ }
		}

		// ── Client / resource roles ──────────────────────────────────────────────
		var resourceAccessClaim = identity.FindFirst("resource_access");
		if (resourceAccessClaim is not null)
		{
			try
			{
				using var doc = JsonDocument.Parse(resourceAccessClaim.Value);
				foreach (var clientEntry in doc.RootElement.EnumerateObject())
				{
					if (clientEntry.Value.TryGetProperty("roles", out var roles))
					{
						foreach (var role in roles.EnumerateArray())
						{
							var roleValue = role.GetString();
							if (!string.IsNullOrEmpty(roleValue))
								identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
						}
					}
				}
			}
			catch (JsonException) { /* malformed claim — skip */ }
		}
	}
}