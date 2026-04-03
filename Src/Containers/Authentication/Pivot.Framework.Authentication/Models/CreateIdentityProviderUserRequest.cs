namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral request for creating a managed identity user.
/// </summary>
public sealed class CreateIdentityProviderUserRequest
{
	/// <summary>
	/// Username for the new user.
	/// </summary>
	public string Username { get; set; } = string.Empty;

	/// <summary>
	/// Optional email address.
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// Optional first name.
	/// </summary>
	public string? FirstName { get; set; }

	/// <summary>
	/// Optional last name.
	/// </summary>
	public string? LastName { get; set; }

	/// <summary>
	/// Whether the user should be enabled immediately.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Optional initial roles.
	/// </summary>
	public IReadOnlyCollection<string> Roles { get; set; } = [];
}
