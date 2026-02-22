namespace Templates.Core.Caching.Abstractions;

/// <summary>
/// A serializable snapshot of the claims extracted from a valid JWT.
/// </summary>
public sealed class CachedTokenClaims
{
	public string UserId { get; init; } = string.Empty;
	public string Username { get; init; } = string.Empty;
	public string Email { get; init; } = string.Empty;

	/// <summary>Flattened realm + client roles.</summary>
	public List<string> Roles { get; init; } = [];

	/// <summary>All raw claims as key→value pairs for full fidelity.</summary>
	public Dictionary<string, string> AllClaims { get; init; } = [];
}