namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral request for updating a managed identity user.
/// </summary>
public sealed class UpdateIdentityProviderUserRequest
{
	/// <summary>
	/// Optional email address.
	/// </summary>
	public string? Email { get; set; }

    /// <summary>
    /// Optional enabled flag.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Optional username override.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Optional last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Optional first name.
    /// </summary>
    public string? FirstName { get; set; }
}