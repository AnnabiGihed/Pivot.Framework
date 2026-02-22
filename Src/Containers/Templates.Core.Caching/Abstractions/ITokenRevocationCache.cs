namespace Templates.Core.Caching.Abstractions;

// ─────────────────────────────────────────────────────────────────────────────
// ITokenRevocationCache
// When a user logs out, their token(s) are added to a Redis blacklist.
// The JWT bearer handler checks this before accepting a token.
// This enables real-time logout even for tokens with a long remaining lifetime.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Token revocation blacklist backed by Redis.
/// Allows immediate logout even before the JWT expires.
/// </summary>
public interface ITokenRevocationCache
{
	/// <summary>
	/// Revokes all tokens for a given user (subject claim).
	/// Stores a "revoke all before" timestamp — any token issued before this time is rejected.
	/// </summary>
	Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);

	/// <summary>
	/// Returns <c>true</c> if the token has been explicitly revoked.
	/// </summary>
	Task<bool> IsRevokedAsync(string accessToken, CancellationToken ct = default);

	/// <summary>
	/// Marks a token as revoked. The entry auto-expires when the token would have expired.
	/// </summary>
	Task RevokeAsync(string accessToken, DateTimeOffset tokenExpiresAt, CancellationToken ct = default);

	/// <summary>
	/// Returns true if the token was issued before the user's global revocation timestamp.
	/// </summary>
	Task<bool> IsIssuedBeforeRevocationAsync(string userId, DateTimeOffset tokenIssuedAt, CancellationToken ct = default);
}