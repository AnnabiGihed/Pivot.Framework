namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral session model for authentication-centric microservices.
/// </summary>
public sealed class AuthSession
{
	/// <summary>
	/// Subject identifier associated with the session.
	/// </summary>
	public string? SubjectId { get; set; }

	/// <summary>
	/// Username associated with the session.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Access token tracked by the session.
	/// </summary>
	public string? AccessToken { get; set; }

	/// <summary>
	/// Refresh token tracked by the session.
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// Session expiry timestamp.
	/// </summary>
	public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Session identifier.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
}