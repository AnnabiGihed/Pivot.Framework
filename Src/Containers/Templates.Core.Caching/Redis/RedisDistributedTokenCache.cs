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

	#region Dependencies
	private readonly ICacheService _cache;
	#endregion

	#region Constructor
	public RedisDistributedTokenCache(ICacheService cache)
	{
		_cache = cache;
	}
	#endregion

	#region Public Methods
	public Task InvalidateAsync(string accessToken, CancellationToken ct = default)
	{
		return _cache.RemoveAsync(BuildKey(accessToken), ct);
	}

	public Task<CachedTokenClaims?> GetClaimsAsync(string accessToken, CancellationToken ct = default)
	{
		return _cache.GetAsync<CachedTokenClaims>(BuildKey(accessToken), ct);
	}

	public Task SetClaimsAsync(string accessToken, CachedTokenClaims claims, DateTimeOffset tokenExpiresAt, CancellationToken ct = default)
	{
		var ttl = tokenExpiresAt - DateTimeOffset.UtcNow;
		if (ttl <= TimeSpan.Zero)
			return Task.CompletedTask;

		return _cache.SetAsync(BuildKey(accessToken), claims, ttl, ct);
	}
	#endregion

	#region Private helpers
	private static string BuildKey(string accessToken)
	{
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
		return $"{Prefix}{Convert.ToHexString(hash)}";
	}
	#endregion
}