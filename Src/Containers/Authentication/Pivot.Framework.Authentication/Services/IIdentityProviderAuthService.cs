using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral backend authentication service abstraction.
/// </summary>
public interface IIdentityProviderAuthService
{
	/// <summary>
	/// Builds a provider authorization URL.
	/// </summary>
	Task<AuthAuthorizationResult> BuildAuthorizationUrlAsync(AuthAuthorizationRequest request, CancellationToken ct = default);

	/// <summary>
	/// Exchanges an authorization code for tokens.
	/// </summary>
	Task<AuthTokenResponse> ExchangeAuthorizationCodeAsync(AuthCodeExchangeRequest request, CancellationToken ct = default);

	/// <summary>
	/// Refreshes tokens using a refresh token.
	/// </summary>
	Task<AuthTokenResponse> RefreshTokenAsync(AuthRefreshTokenRequest request, CancellationToken ct = default);

	/// <summary>
	/// Executes provider logout/revocation semantics.
	/// </summary>
	Task LogoutAsync(AuthLogoutRequest request, CancellationToken ct = default);

	/// <summary>
	/// Resolves the user's profile from the provider.
	/// </summary>
	Task<IdentityProviderUser> GetUserProfileAsync(string accessToken, CancellationToken ct = default);
}
