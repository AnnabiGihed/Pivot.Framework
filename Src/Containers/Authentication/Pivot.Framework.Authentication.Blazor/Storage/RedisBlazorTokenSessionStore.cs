using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Pivot.Framework.Authentication.Blazor.Storage;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Redis-backed server-side token session store for Blazor.
///              Tokens are stored entirely on the server — the browser only ever receives
///              an opaque session cookie. TTL is derived from the token's actual expiry
///              so sessions auto-evict without a background job.
/// </summary>
internal sealed class RedisBlazorTokenSessionStore : IBlazorTokenSessionStore
{
	#region Constants
	/// <summary>The Redis key prefix for all Blazor session entries.</summary>
	private const string Prefix = "blazor:session:";

	/// <summary>
	/// Minimum TTL for an incomplete login-flow session (PKCE state before callback).
	/// After this the user must start over.
	/// </summary>
	private static readonly TimeSpan FlowStateTtl = TimeSpan.FromMinutes(10);
	#endregion

	#region Dependencies
	private readonly IDistributedCache _cache;
	/// <summary>Shared JSON serializer options: camelCase naming with null values omitted.</summary>
	private static readonly JsonSerializerOptions JsonOpts = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
	};
	#endregion

	#region Constructor
	/// <summary>
	/// Initialises a new instance of <see cref="RedisBlazorTokenSessionStore"/> with the provided distributed cache.
	/// </summary>
	/// <param name="cache">The distributed cache implementation backed by Redis.</param>
	public RedisBlazorTokenSessionStore(IDistributedCache cache)
	{
		_cache = cache;
	}
	#endregion

	#region IBlazorTokenSessionStore
	/// <inheritdoc />
	public Task RemoveAsync(string sessionId, CancellationToken ct = default)
	{
		return _cache.RemoveAsync(BuildKey(sessionId), ct);
	}
	/// <inheritdoc />
	public async Task<BlazorTokenSession?> GetAsync(string sessionId, CancellationToken ct = default)
	{
		var bytes = await _cache.GetAsync(BuildKey(sessionId), ct);

		if (bytes is null || bytes.Length == 0)
			return null;

		return JsonSerializer.Deserialize<BlazorTokenSession>(bytes, JsonOpts);
	}
	/// <inheritdoc />
	public async Task SaveAsync(string sessionId, BlazorTokenSession session, CancellationToken ct = default)
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(session, JsonOpts);

		var ttl = session.HasTokens && session.RefreshTokenExpiresAt.HasValue
			? session.RefreshTokenExpiresAt.Value - DateTimeOffset.UtcNow
			: session.HasTokens && session.ExpiresAt.HasValue
				? session.ExpiresAt.Value - DateTimeOffset.UtcNow + TimeSpan.FromHours(1)
				: FlowStateTtl;

		if (ttl <= TimeSpan.Zero)
			ttl = FlowStateTtl;

		await _cache.SetAsync(BuildKey(sessionId), bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
	}
	#endregion

	#region Private helpers
	/// <summary>
	/// Builds the Redis key for a given session ID by prepending the <see cref="Prefix"/>.
	/// </summary>
	/// <param name="sessionId">The opaque session ID.</param>
	/// <returns>The fully qualified Redis key string.</returns>
	private static string BuildKey(string sessionId) => $"{Prefix}{sessionId}";
	#endregion
}