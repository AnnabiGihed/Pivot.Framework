using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Default implementation of <see cref="ICurrentUser"/> that extracts
///              the authenticated user's claims from the current HTTP context.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
	#region Dependencies
	private readonly IHttpContextAccessor _accessor;
	#endregion

	#region Constructor
	/// <summary>
	/// Initialises a new instance of <see cref="CurrentUser"/> with the provided HTTP context accessor.
	/// </summary>
	/// <param name="accessor">The HTTP context accessor used to resolve the current request's user principal.</param>
	public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;
	#endregion

	#region Private helpers
	/// <summary>
	/// The claims principal of the current HTTP request user, or <c>null</c> if no HTTP context is available.
	/// </summary>
	private ClaimsPrincipal? User => _accessor.HttpContext?.User;
	#endregion

	#region ICurrentUser
	/// <inheritdoc />
	public Guid? UserId
	{
		get
		{
			var raw = User?.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? User?.FindFirstValue("sub");

			return Guid.TryParse(raw, out var guid) ? guid : null;
		}
	}

	/// <inheritdoc />
	public string? DisplayName
	{
		get
		{
			var name = User?.FindFirstValue("name");
			if (name is not null)
				return name;

			var given = User?.FindFirstValue("given_name");
			var family = User?.FindFirstValue("family_name");

			if (given is not null)
				return string.IsNullOrWhiteSpace(family) ? given : $"{given} {family}".Trim();

			return Username;
		}
	}

    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            return User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList() ?? [];
        }
    }

    /// <inheritdoc />
    public ClaimsPrincipal? Principal => User;

	/// <inheritdoc />
	public bool IsInRole(string role) => User?.IsInRole(role) == true;

	/// <inheritdoc />
	public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

	/// <inheritdoc />
	public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");

	/// <inheritdoc />
	public string? Username => User?.FindFirstValue("preferred_username") ?? User?.FindFirstValue(ClaimTypes.Name);
	#endregion
}