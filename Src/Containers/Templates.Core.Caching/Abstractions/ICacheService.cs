namespace Templates.Core.Caching.Abstractions;

/// <summary>
/// Generic typed distributed cache. Wraps <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>
/// with JSON serialization so callers never touch raw byte arrays.
///
/// Register via <see cref="Extensions.RedisCachingExtensions.AddRedisCache"/>.
/// </summary>
public interface ICacheService
{
	/// <summary>Gets a cached value. Returns <c>null</c> if the key does not exist.</summary>
	Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

	/// <summary>Sets a value with an absolute expiry.</summary>
	Task SetAsync<T>(string key, T value, TimeSpan absoluteExpiry, CancellationToken ct = default)
		where T : class;

	/// <summary>Removes a key (no-op if absent).</summary>
	Task RemoveAsync(string key, CancellationToken ct = default);

	/// <summary>Returns <c>true</c> if the key exists.</summary>
	Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}