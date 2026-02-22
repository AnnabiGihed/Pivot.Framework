using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Templates.Core.Authentication.Models;

/// <inheritdoc />
public sealed class CurrentUser : ICurrentUser
{
	#region Fields
	public string? DisplayName
	{
		get
		{
			// 1. Try the "name" claim (full name set by Keycloak)
			var name = User?.FindFirstValue("name");
			if (name is not null) return name;

			// 2. Try composing given_name + family_name
			var given = User?.FindFirstValue("given_name");
			var family = User?.FindFirstValue("family_name");
			if (given is not null)
				return string.IsNullOrWhiteSpace(family)
					? given
					: $"{given} {family}".Trim();

			// 3. Fall back to preferred_username
			return Username;
		}
	}
	public IReadOnlyList<string> Roles => User?.Claims
	.Where(c => c.Type == ClaimTypes.Role)
	.Select(c => c.Value)
	.ToList() ?? [];
	public ClaimsPrincipal? Principal => User;
	private ClaimsPrincipal? User => _accessor.HttpContext?.User;
	public bool IsInRole(string role) => User?.IsInRole(role) == true;
	public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
	public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");
	public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");
	public string? Username => User?.FindFirstValue("preferred_username") ?? User?.FindFirstValue(ClaimTypes.Name);
	#endregion

	#region Dependencies
	private readonly IHttpContextAccessor _accessor;
	#endregion

	#region Constructor
	public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;
	#endregion
}