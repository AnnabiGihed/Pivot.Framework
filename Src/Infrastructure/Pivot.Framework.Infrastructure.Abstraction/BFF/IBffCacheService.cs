namespace Pivot.Framework.Infrastructure.Abstraction.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Cache service for BFF response caching with TTL-based eligibility.
///              Respects the never-cached / short-TTL / medium-TTL classification
///              defined in BffCacheOptions.
/// </summary>
public interface IBffCacheService
{
	/// <summary>Gets a cached value, or null if not cached or expired.</summary>
	Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

	/// <summary>Sets a cached value with the configured TTL for the resource key.</summary>
	Task SetAsync<T>(string key, T value, CancellationToken ct = default) where T : class;

	/// <summary>Invalidates a specific cache entry.</summary>
	Task InvalidateAsync(string key, CancellationToken ct = default);

	/// <summary>Invalidates all cache entries matching a prefix.</summary>
	Task InvalidateByPrefixAsync(string prefix, CancellationToken ct = default);
}
