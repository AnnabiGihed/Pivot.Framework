using System.Text.Json;
using Templates.Core.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Templates.Core.Caching.Redis;

/// <inheritdoc />
internal sealed class RedisCacheService : ICacheService
{
	#region Dependencies
	private readonly IDistributedCache _cache;
	#endregion

	#region Constants
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
	};
	#endregion

	#region Constructor
	public RedisCacheService(IDistributedCache cache)
	{
		_cache = cache;
	}
	#endregion

	#region Public Methods
	/// <inheritdoc />
	public Task RemoveAsync(string key, CancellationToken ct = default)
	{
		return _cache.RemoveAsync(key, ct);
	}

	/// <inheritdoc />
	public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
		where T : class
	{
		var bytes = await _cache.GetAsync(key, ct);
		if (bytes is null || bytes.Length == 0)
			return null;

		return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
	}

	/// <inheritdoc />
	public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
	{
		var bytes = await _cache.GetAsync(key, ct);
		return bytes is not null && bytes.Length > 0;
	}

	/// <inheritdoc />
	public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpiry, CancellationToken ct = default)
		where T : class
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
		await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = absoluteExpiry
		}, ct);
	}
	#endregion
}