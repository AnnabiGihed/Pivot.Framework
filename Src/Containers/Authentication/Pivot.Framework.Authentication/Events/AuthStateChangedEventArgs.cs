using System.Security.Claims;

namespace Pivot.Framework.Authentication.Events;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Event arguments for <see cref="IKeycloakAuthService.AuthStateChanged"/>,
///              carrying the current authentication flag and the associated user principal.
/// </summary>
public sealed class AuthStateChangedEventArgs : EventArgs
{
	#region Properties
	/// <summary>
	/// Indicates whether the user is currently authenticated.
	/// </summary>
	public bool IsAuthenticated { get; }

	/// <summary>
	/// The claims principal representing the authenticated user, or <c>null</c> if unauthenticated.
	/// </summary>
	public ClaimsPrincipal? User { get; }
	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of <see cref="AuthStateChangedEventArgs"/>.
	/// </summary>
	/// <param name="isAuthenticated">Whether the user is authenticated.</param>
	/// <param name="user">The claims principal, or <c>null</c> if unauthenticated.</param>
	public AuthStateChangedEventArgs(bool isAuthenticated, ClaimsPrincipal? user)
	{
        User = user;
        IsAuthenticated = isAuthenticated;
	}
	#endregion
}