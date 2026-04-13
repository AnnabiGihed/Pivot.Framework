using System.Security.Claims;
using Pivot.Framework.Authentication.Events;
using Pivot.Framework.Authentication.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pivot.Framework.Authentication.Maui.AuthenticationStateProvider;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Blazor <see cref="Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider"/> backed by <see cref="IKeycloakAuthService"/>.
///              Integrates Keycloak authentication state into Blazor primitives such as
///              <c>&lt;AuthorizeView&gt;</c>, <c>[Authorize]</c> and <c>CascadingAuthenticationState</c>.
/// </summary>
public sealed class KeycloakAuthStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, IDisposable
{
	#region Fields
	/// <summary>
	/// The current authentication state cached for this circuit.
	/// </summary>
	private AuthenticationState _current;
	#endregion

	#region Constants
	/// <summary>
	/// A reusable unauthenticated state returned when no user is logged in.
	/// </summary>
	private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
    #endregion

    #region Dependencies
    /// <summary>
    /// The Keycloak authentication service that provides the current user and authentication status.
    /// </summary>
    private readonly IKeycloakAuthService _auth;
	#endregion

	#region Constructor
	/// <summary>
	/// Initialises a new instance of <see cref="KeycloakAuthStateProvider"/> and subscribes to auth state changes.
	/// </summary>
	/// <param name="auth">The Keycloak authentication service that drives the Blazor authentication state.</param>
	public KeycloakAuthStateProvider(IKeycloakAuthService auth)
	{
		_auth = auth;
		_auth.AuthStateChanged += OnAuthStateChanged;
		_current = _auth.IsAuthenticated && _auth.User is not null ? new AuthenticationState(_auth.User) : Anonymous;
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		return Task.FromResult(_current);
	}
	#endregion

	#region Private helpers
	/// <summary>
	/// Handles the <see cref="IKeycloakAuthService.AuthStateChanged"/> event and notifies Blazor components of the updated state.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The event arguments containing the new authentication state.</param>
	private void OnAuthStateChanged(object? sender, AuthStateChangedEventArgs e)
	{
		_current = e.IsAuthenticated && e.User is not null ? new AuthenticationState(e.User) : Anonymous;

		NotifyAuthenticationStateChanged(Task.FromResult(_current));
	}
	#endregion

	#region IDisposable
	/// <inheritdoc />
	public void Dispose()
	{
		_auth.AuthStateChanged -= OnAuthStateChanged;
	}
	#endregion
}