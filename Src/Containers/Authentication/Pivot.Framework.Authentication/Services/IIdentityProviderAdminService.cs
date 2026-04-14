using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral administrative service abstraction for identity management.
/// </summary>
public interface IIdentityProviderAdminService
{
	#region Methods
	/// <summary>
	/// Gets a user by provider identifier.
	/// </summary>
	Task<IdentityProviderUser?> GetUserByIdAsync(string userId, CancellationToken ct = default);

	/// <summary>
	/// Gets a user by email address.
	/// </summary>
	Task<IdentityProviderUser?> GetUserByEmailAsync(string email, CancellationToken ct = default);

	/// <summary>
	/// Creates a new user and returns the provider identifier.
	/// </summary>
	Task<string> CreateUserAsync(CreateIdentityProviderUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Assigns roles to a user.
    /// </summary>
    Task AssignRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken ct = default);

    /// <summary>
    /// Removes roles from a user.
    /// </summary>
    Task RemoveRolesAsync(string userId, IReadOnlyCollection<string> roles, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateUserAsync(string userId, UpdateIdentityProviderUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Lists provider users optionally filtered by a search term.
    /// </summary>
    Task<IReadOnlyCollection<IdentityProviderUser>> GetUsersAsync(string? search = null, CancellationToken ct = default);
	#endregion
}