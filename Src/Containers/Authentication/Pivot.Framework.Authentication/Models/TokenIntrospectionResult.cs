namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral token introspection result.
/// </summary>
public sealed class TokenIntrospectionResult
{
	#region Properties
	/// <summary>
	/// Whether the token is active.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Username when exposed by the provider.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Client identifier.
	/// </summary>
	public string? ClientId { get; set; }

    /// <summary>
    /// Token subject identifier.
    /// </summary>
    public string? SubjectId { get; set; }

    /// <summary>
    /// Expiration timestamp when exposed by the provider.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

	/// <summary>
	/// Granted scopes.
	/// </summary>
	public IReadOnlyCollection<string> Scopes { get; set; } = [];

	/// <summary>
	/// Provider roles when exposed by the provider.
	/// </summary>
	public IReadOnlyCollection<string> Roles { get; set; } = [];
	#endregion
}
