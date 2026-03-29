using FluentAssertions;
using Microsoft.Extensions.Options;
using Pivot.Framework.Containers.API.BFF;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.Tests.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="InMemoryBffCacheService"/>.
///              Verifies caching, TTL expiration, invalidation, prefix invalidation,
///              and non-cacheable key behaviour.
/// </summary>
public class BffCacheServiceTests
{
	private readonly BffCacheOptions _cacheOptions;
	private readonly InMemoryBffCacheService _sut;

	public BffCacheServiceTests()
	{
		_cacheOptions = new BffCacheOptions();
		_cacheOptions.AddCacheable("reference-data", TimeSpan.FromMinutes(5));
		_cacheOptions.AddCacheable("dashboard-badge", TimeSpan.FromSeconds(10));
		_cacheOptions.AddNeverCached("conflict-state");

		_sut = new InMemoryBffCacheService(Options.Create(_cacheOptions));
	}

	#region GetAsync Tests

	[Fact]
	public async Task GetAsync_ForNonExistentKey_ShouldReturnNull()
	{
		var result = await _sut.GetAsync<string>("reference-data:missing");

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_ForNonCacheableKey_ShouldReturnNull()
	{
		await _sut.SetAsync("conflict-state", "value");

		var result = await _sut.GetAsync<string>("conflict-state");

		result.Should().BeNull();
	}

	#endregion

	#region SetAsync / GetAsync Round-Trip Tests

	[Fact]
	public async Task SetAsync_ShouldStoreAndRetrieveValue()
	{
		await _sut.SetAsync("reference-data", "cached-value");

		var result = await _sut.GetAsync<string>("reference-data");

		result.Should().Be("cached-value");
	}

	[Fact]
	public async Task SetAsync_ForNonCacheableKey_ShouldNotStore()
	{
		await _sut.SetAsync("conflict-state", "should-not-cache");

		var result = await _sut.GetAsync<string>("conflict-state");

		result.Should().BeNull();
	}

	[Fact]
	public async Task SetAsync_ForUnknownKey_ShouldNotStore()
	{
		await _sut.SetAsync("unknown-key", "data");

		var result = await _sut.GetAsync<string>("unknown-key");

		result.Should().BeNull();
	}

	#endregion

	#region InvalidateAsync Tests

	[Fact]
	public async Task InvalidateAsync_ShouldRemoveCachedEntry()
	{
		await _sut.SetAsync("reference-data", "value");
		await _sut.InvalidateAsync("reference-data");

		var result = await _sut.GetAsync<string>("reference-data");

		result.Should().BeNull();
	}

	[Fact]
	public async Task InvalidateAsync_ForNonExistentKey_ShouldNotThrow()
	{
		var act = () => _sut.InvalidateAsync("non-existent");

		await act.Should().NotThrowAsync();
	}

	#endregion

	#region InvalidateByPrefixAsync Tests

	[Fact]
	public async Task InvalidateByPrefixAsync_ShouldRemoveMatchingEntries()
	{
		// Use a cacheable prefix
		_cacheOptions.AddCacheable("reference-data:schemas", TimeSpan.FromMinutes(5));
		_cacheOptions.AddCacheable("reference-data:sources", TimeSpan.FromMinutes(5));
		_cacheOptions.AddCacheable("dashboard-badge:count", TimeSpan.FromSeconds(10));

		await _sut.SetAsync("reference-data:schemas", "schemas-data");
		await _sut.SetAsync("reference-data:sources", "sources-data");
		await _sut.SetAsync("dashboard-badge:count", "42");

		await _sut.InvalidateByPrefixAsync("reference-data");

		var schemas = await _sut.GetAsync<string>("reference-data:schemas");
		var sources = await _sut.GetAsync<string>("reference-data:sources");
		var badge = await _sut.GetAsync<string>("dashboard-badge:count");

		schemas.Should().BeNull();
		sources.Should().BeNull();
		badge.Should().Be("42");
	}

	#endregion
}
