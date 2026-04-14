namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral refresh-token request.
/// </summary>
public sealed class AuthRefreshTokenRequest
{
    #region Properties
    /// <summary>
    /// Optional session identifier used when persisting auth sessions.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Refresh token used to obtain a new access token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    #endregion
}