namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral request used to exchange an authorization code for tokens.
/// </summary>
public sealed class AuthCodeExchangeRequest
{
    #region Properties
    /// <summary>
    /// Optional session identifier used when persisting auth sessions.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Optional PKCE code verifier.
    /// </summary>
    public string? CodeVerifier { get; set; }

    /// <summary>
    /// Authorization code returned by the identity provider.
    /// </summary>
    public string Code { get; set; } = string.Empty;

	/// <summary>
	/// Redirect URI used during the login flow.
	/// </summary>
	public string RedirectUri { get; set; } = string.Empty;
    #endregion
}
