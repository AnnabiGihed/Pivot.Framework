using System.Security.Claims;

namespace Templates.Core.Authentication.Maui.Events;

/// <summary>Arguments for the <see cref="IKeycloakAuthService.AuthStateChanged"/> event.</summary>
public sealed class AuthStateChangedEventArgs : EventArgs
{
	public bool IsAuthenticated { get; }
	public ClaimsPrincipal? User { get; }

	public AuthStateChangedEventArgs(bool isAuthenticated, ClaimsPrincipal? user)
	{
		IsAuthenticated = isAuthenticated;
		User = user;
	}
}