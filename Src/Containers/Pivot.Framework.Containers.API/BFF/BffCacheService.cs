using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : In-memory implementation of <see cref="IBffCacheService"/> using ConcurrentDictionary.
///              Respects the TTL-based eligibility rules defined in <see cref="BffCacheOptions"/>.
///              For distributed deployments, replace with a Redis-backed implementation.
/// </summary>
public sealed class InMemoryBffCacheService : IBffCacheService
{
	private readonly BffCacheOptions _options;
	private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

	public InMemoryBffCacheService(IOptions<BffCacheOptions> options)
	{
		_options = options.Value;
	}

	/// <inheritdoc />
	public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
	{
		if (!_options.IsCacheable(key))
			return Task.FromResult<T?>(null);

		if (_cache.TryGetValue(key, out var item) && item.ExpiresAtUtc > DateTime.UtcNow)
			return Task.FromResult(item.Value as T);

		_cache.TryRemove(key, out _);
		return Task.FromResult<T?>(null);
	}

	/// <inheritdoc />
	public Task SetAsync<T>(string key, T value, CancellationToken ct = default) where T : class
	{
		var ttl = _options.GetTtl(key);
		if (ttl is null) return Task.CompletedTask;

		_cache[key] = new CacheItem
		{
			Value = value,
			ExpiresAtUtc = DateTime.UtcNow.Add(ttl.Value)
		};
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task InvalidateAsync(string key, CancellationToken ct = default)
	{
		_cache.TryRemove(key, out _);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task InvalidateByPrefixAsync(string prefix, CancellationToken ct = default)
	{
		var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
		foreach (var key in keysToRemove)
			_cache.TryRemove(key, out _);
		return Task.CompletedTask;
	}

	private sealed class CacheItem
	{
		public required object Value { get; init; }
		public DateTime ExpiresAtUtc { get; init; }
	}
}
