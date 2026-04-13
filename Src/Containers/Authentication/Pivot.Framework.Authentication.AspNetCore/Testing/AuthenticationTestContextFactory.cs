using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Pivot.Framework.Authentication.AspNetCore.Testing;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Lightweight helpers for auth-related unit and integration tests.
/// </summary>
public static class AuthenticationTestContextFactory
{
	#region Public Methods
	/// <summary>
	/// Creates an authenticated claims principal with the supplied claims and roles.
	/// </summary>
	public static ClaimsPrincipal CreatePrincipal(string? subjectId = null, string? username = null, string? email = null, IEnumerable<string>? roles = null, IEnumerable<Claim>? additionalClaims = null, string authenticationType = "TestAuthentication")
	{
		var claims = new List<Claim>();

		if (!string.IsNullOrWhiteSpace(subjectId))
			claims.Add(new Claim(ClaimTypes.NameIdentifier, subjectId));

		if (!string.IsNullOrWhiteSpace(username))
			claims.Add(new Claim("preferred_username", username));

		if (!string.IsNullOrWhiteSpace(email))
			claims.Add(new Claim(ClaimTypes.Email, email));

		if (roles is not null)
			claims.AddRange(roles.Where(role => !string.IsNullOrWhiteSpace(role)).Select(role => new Claim(ClaimTypes.Role, role)));

		if (additionalClaims is not null)
			claims.AddRange(additionalClaims);

		return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
	}

	/// <summary>
	/// Creates an HTTP context containing the supplied principal.
	/// </summary>
	public static DefaultHttpContext CreateHttpContext(ClaimsPrincipal? principal = null)
	{
		return new DefaultHttpContext
		{
			User = principal ?? CreatePrincipal()
		};
	}
	#endregion
}