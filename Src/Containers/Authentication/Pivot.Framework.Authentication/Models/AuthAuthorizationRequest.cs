namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral authorization request used to build an interactive login URL.
/// </summary>
public sealed class AuthAuthorizationRequest
{
    #region Properties
    /// <summary>
    /// Optional explicit scope list. Falls back to provider defaults when omitted.
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// Optional OAuth2 state value for CSRF protection.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Optional prompt override.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Optional login hint.
    /// </summary>
    public string? LoginHint { get; set; }

    /// <summary>
    /// Optional PKCE code challenge.
    /// </summary>
    public string? CodeChallenge { get; set; }

    /// <summary>
    /// Redirect URI that should receive the authorization callback.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Optional PKCE code challenge method. Defaults to S256.
    /// </summary>
    public string CodeChallengeMethod { get; set; } = "S256";
    #endregion
}
