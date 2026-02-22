using System.Text;
using System.Security.Cryptography;
using Templates.Core.Caching.Abstractions;

namespace Templates.Core.Caching.Redis;

/// <inheritdoc />
internal sealed class RedisDistributedTokenCache : IDistributedTokenCache
{
	#region Constants
	private const string Prefix = "tkn:claims:";
	#endregion

	#region Fields
	private readonly ICacheService _cache;
	#endregion

	public RedisDistributedTokenCache(ICacheService cache) => _cache = cache;

	public Task<CachedTokenClaims?> GetClaimsAsync(string accessToken, CancellationToken ct = default) =>
		_cache.GetAsync<CachedTokenClaims>(BuildKey(accessToken), ct);

	public Task SetClaimsAsync(string accessToken, CachedTokenClaims claims, DateTimeOffset tokenExpiresAt, CancellationToken ct = default)
	{
		var ttl = tokenExpiresAt - DateTimeOffset.UtcNow;
		if (ttl <= TimeSpan.Zero) return Task.CompletedTask; // already expired

		return _cache.SetAsync(BuildKey(accessToken), claims, ttl, ct);
	}

	public Task InvalidateAsync(string accessToken, CancellationToken ct = default) =>
		_cache.RemoveAsync(BuildKey(accessToken), ct);

	// ─── Key derivation ───────────────────────────────────────────────────────
	// We never store the raw token in Redis — only a SHA-256 hash.
	// This prevents key material from being recoverable even if Redis is compromised.

	private static string BuildKey(string accessToken)
	{
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
		return $"{Prefix}{Convert.ToHexString(hash)}";
	}
}