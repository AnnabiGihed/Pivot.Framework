namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral role model.
/// </summary>
public sealed class IdentityProviderRole
{
	/// <summary>
	/// Optional role description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Optional provider-specific identifier.
	/// </summary>
	public string? ProviderRoleId { get; set; }

    /// <summary>
    /// Role name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}