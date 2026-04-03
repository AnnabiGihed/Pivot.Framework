namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral logout request.
/// </summary>
public sealed class AuthLogoutRequest
{
	/// <summary>
	/// Optional refresh token to revoke/terminate.
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// Optional ID token hint forwarded to the identity provider.
	/// </summary>
	public string? IdTokenHint { get; set; }

	/// <summary>
	/// Optional session identifier to clear from session storage.
	/// </summary>
	public string? SessionId { get; set; }
}
