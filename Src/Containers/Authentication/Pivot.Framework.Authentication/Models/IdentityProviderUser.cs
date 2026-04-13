namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral identity user model.
/// </summary>
public sealed class IdentityProviderUser
{
    #region Properties
    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Last/family name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// First/given name.
    /// </summary>
    public string? FirstName { get; set; }

	/// <summary>
	/// Display name when available.
	/// </summary>
	public string? DisplayName { get; set; }

	/// <summary>
	/// Whether the account is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Provider-specific user identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

	/// <summary>
    /// Provider roles associated with the user.
    /// </summary>
    public IReadOnlyCollection<IdentityProviderRole> Roles { get; set; } = [];

	/// <summary>
	/// Claims associated with the user.
	/// </summary>
	public IReadOnlyCollection<IdentityProviderClaim> Claims { get; set; } = [];
    #endregion
}
