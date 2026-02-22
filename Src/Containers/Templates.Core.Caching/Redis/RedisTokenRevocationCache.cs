using System.Text;
using System.Security.Cryptography;
using Templates.Core.Caching.Abstractions;

namespace Templates.Core.Caching.Redis;

/// <inheritdoc />
internal sealed class RedisTokenRevocationCache : ITokenRevocationCache
{
	#region Constants
	// Redis key patterns:
	//   tkn:revoked:<sha256-of-token>   → "1" (individual token revoked)
	//   tkn:revoke-all:<userId>         → UnixSeconds (revoke all tokens issued before this time)

	private const string RevokedPrefix = "tkn:revoked:";
	private const string RevokeAllPrefix = "tkn:revoke-all:";
	#endregion

	#region Dependencies
	private readonly ICacheService _cache;
	#endregion

	#region Constructor
	public RedisTokenRevocationCache(ICacheService cache) => _cache = cache;
	#endregion

	#region Key helpers
	// Reuse the same SHA256 approach as RedisDistributedTokenCache
	private static string BuildClaimsKey(string accessToken) => $"tkn:claims:{HashToken(accessToken)}";
	private static string BuildRevokedKey(string accessToken) => $"{RevokedPrefix}{HashToken(accessToken)}";

	private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
	#endregion

	#region Internal DTOs
	private sealed class RevokedSentinel { public string V { get; init; } = "1"; }

	private sealed class RevokeAllSentinel
	{
		public long RevokedAtUnix { get; init; }
	}
	#endregion

	#region Global user revocation
	public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
	{
		// Store the current UTC timestamp. Any JWT with iat < this value is rejected.
		var sentinel = new RevokeAllSentinel
		{
			RevokedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
		};

		// Keep this for 90 days (max realistic refresh token lifetime)
		await _cache.SetAsync($"{RevokeAllPrefix}{userId}", sentinel, TimeSpan.FromDays(90), ct);
	}

	public async Task<bool> IsIssuedBeforeRevocationAsync(string userId, DateTimeOffset tokenIssuedAt, CancellationToken ct = default)
	{
		var sentinel = await _cache.GetAsync<RevokeAllSentinel>(
			$"{RevokeAllPrefix}{userId}", ct);

		if (sentinel is null) return false;

		return tokenIssuedAt.ToUnixTimeSeconds() < sentinel.RevokedAtUnix;
	}
	#endregion

	#region Individual token revocation
	public async Task<bool> IsRevokedAsync(string accessToken, CancellationToken ct = default) =>
		await _cache.ExistsAsync(BuildRevokedKey(accessToken), ct);

	public async Task RevokeAsync(string accessToken, DateTimeOffset tokenExpiresAt, CancellationToken ct = default)
	{
		var ttl = tokenExpiresAt - DateTimeOffset.UtcNow;
		if (ttl <= TimeSpan.Zero) return; // already expired — no point storing

		// Store a tiny sentinel — value doesn't matter, key existence is the flag
		await _cache.SetAsync(BuildRevokedKey(accessToken), new RevokedSentinel(), ttl, ct);

		// Also invalidate from token claims cache
		await _cache.RemoveAsync(BuildClaimsKey(accessToken), ct);
	}
	#endregion
}