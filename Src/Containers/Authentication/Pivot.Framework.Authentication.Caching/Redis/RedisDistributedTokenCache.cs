using System.Text;
using System.Security.Cryptography;
using Pivot.Framework.Authentication.Caching.Abstractions;
using Pivot.Framework.Infrastructure.Caching.Abstractions;

namespace Pivot.Framework.Authentication.Caching.Redis;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Redis-backed implementation of <see cref="IDistributedTokenCache"/>.
///              Keys are derived from SHA-256(access_token) — the raw token is never stored.
///              Delegates all Redis I/O to <see cref="ICacheService"/>.
/// </summary>
internal sealed class RedisDistributedTokenCache : IDistributedTokenCache
{
	#region Constants
	/// <summary>The Redis key prefix for all cached token claim entries.</summary>
	private const string Prefix = "tkn:claims:";
	#endregion

	#region Dependencies
	private readonly ICacheService _cache;
	#endregion

	#region Constructor
	/// <summary>
	/// Initialises a new instance of <see cref="RedisDistributedTokenCache"/> with the provided cache service.
	/// </summary>
	/// <param name="cache">The Redis-backed cache service used for storing token claims.</param>
	public RedisDistributedTokenCache(ICacheService cache)
	{
		ArgumentNullException.ThrowIfNull(cache);
		_cache = cache;
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public Task InvalidateAsync(string accessToken, CancellationToken ct = default)
	{
		return _cache.RemoveAsync(BuildKey(accessToken), ct);
	}

	/// <inheritdoc />
	public Task<CachedTokenClaims?> GetClaimsAsync(string accessToken, CancellationToken ct = default)
	{
		return _cache.GetAsync<CachedTokenClaims>(BuildKey(accessToken), ct);
	}

	/// <inheritdoc />
	public Task SetClaimsAsync(string accessToken, CachedTokenClaims claims, DateTimeOffset tokenExpiresAt, CancellationToken ct = default)
	{
		var ttl = tokenExpiresAt - DateTimeOffset.UtcNow;
		if (ttl <= TimeSpan.Zero)
			return Task.CompletedTask;

		return _cache.SetAsync(BuildKey(accessToken), claims, ttl, ct);
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Builds the Redis cache key by computing SHA-256 of the access token and prepending the <see cref="Prefix"/>.
	/// The raw token is never stored as a key.
	/// </summary>
	/// <param name="accessToken">The raw access token to hash.</param>
	/// <returns>The Redis key string.</returns>
	private static string BuildKey(string accessToken)
	{
		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(accessToken));
		return $"{Prefix}{Convert.ToHexString(hash)}";
	}
	#endregion
}