namespace Pivot.Framework.Infrastructure.Abstraction.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Cache eligibility and TTL configuration for BFF response caching.
///              Implements the cache rules defined in MDM spec Section 6E:
///              - Eligible (max 60s TTL): schema catalogs, source registry, steward pools
///              - Eligible (max 10s TTL): queue counts, dashboard badges
///              - Never cached: conflict state, task assignment, allowed actions, quality gates
/// </summary>
public sealed class BffCacheOptions
{
	/// <summary>Cache entries with their TTL configuration.</summary>
	public Dictionary<string, BffCacheEntry> Entries { get; } = new();

	/// <summary>Registers a cacheable resource with a TTL.</summary>
	public BffCacheOptions AddCacheable(string resourceKey, TimeSpan ttl)
	{
		Entries[resourceKey] = new BffCacheEntry { ResourceKey = resourceKey, Ttl = ttl, IsCacheable = true };
		return this;
	}

	/// <summary>Explicitly marks a resource as never-cacheable (request-scoped only).</summary>
	public BffCacheOptions AddNeverCached(string resourceKey)
	{
		Entries[resourceKey] = new BffCacheEntry { ResourceKey = resourceKey, Ttl = TimeSpan.Zero, IsCacheable = false };
		return this;
	}

	/// <summary>Returns whether a resource is eligible for caching.</summary>
	public bool IsCacheable(string resourceKey) =>
		Entries.TryGetValue(resourceKey, out var entry) && entry.IsCacheable;

	/// <summary>Returns the TTL for a cacheable resource, or null if not cacheable.</summary>
	public TimeSpan? GetTtl(string resourceKey) =>
		Entries.TryGetValue(resourceKey, out var entry) && entry.IsCacheable ? entry.Ttl : null;
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Individual cache entry configuration for a BFF resource.
/// </summary>
public sealed class BffCacheEntry
{
	public string ResourceKey { get; init; } = string.Empty;
	public TimeSpan Ttl { get; init; }
	public bool IsCacheable { get; init; }
}
