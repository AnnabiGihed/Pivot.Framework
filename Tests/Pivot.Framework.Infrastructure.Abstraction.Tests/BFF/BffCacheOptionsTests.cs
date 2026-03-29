using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BffCacheOptions"/>.
///              Verifies TTL-based cache eligibility rules including cacheable,
///              never-cached, and unknown resource key scenarios.
/// </summary>
public class BffCacheOptionsTests
{
	#region AddCacheable Tests

	[Fact]
	public void AddCacheable_ShouldRegisterCacheableEntry()
	{
		var options = new BffCacheOptions();
		options.AddCacheable("reference-data", TimeSpan.FromSeconds(60));

		options.IsCacheable("reference-data").Should().BeTrue();
		options.GetTtl("reference-data").Should().Be(TimeSpan.FromSeconds(60));
	}

	[Fact]
	public void AddCacheable_ShouldSupportChaining()
	{
		var options = new BffCacheOptions()
			.AddCacheable("ref", TimeSpan.FromSeconds(60))
			.AddCacheable("badge", TimeSpan.FromSeconds(10));

		options.Entries.Should().HaveCount(2);
	}

	#endregion

	#region AddNeverCached Tests

	[Fact]
	public void AddNeverCached_ShouldRegisterNonCacheableEntry()
	{
		var options = new BffCacheOptions();
		options.AddNeverCached("conflict-state");

		options.IsCacheable("conflict-state").Should().BeFalse();
		options.GetTtl("conflict-state").Should().BeNull();
	}

	#endregion

	#region IsCacheable Tests

	[Fact]
	public void IsCacheable_ForUnknownKey_ShouldReturnFalse()
	{
		var options = new BffCacheOptions();

		options.IsCacheable("unknown-key").Should().BeFalse();
	}

	#endregion

	#region GetTtl Tests

	[Fact]
	public void GetTtl_ForUnknownKey_ShouldReturnNull()
	{
		var options = new BffCacheOptions();

		options.GetTtl("unknown-key").Should().BeNull();
	}

	[Fact]
	public void GetTtl_ForNeverCachedKey_ShouldReturnNull()
	{
		var options = new BffCacheOptions();
		options.AddNeverCached("task-assignment");

		options.GetTtl("task-assignment").Should().BeNull();
	}

	#endregion

	#region Override Tests

	[Fact]
	public void AddCacheable_ShouldOverridePreviousEntry()
	{
		var options = new BffCacheOptions();
		options.AddCacheable("key", TimeSpan.FromSeconds(60));
		options.AddCacheable("key", TimeSpan.FromSeconds(30));

		options.GetTtl("key").Should().Be(TimeSpan.FromSeconds(30));
	}

	[Fact]
	public void AddNeverCached_ShouldOverrideCacheableEntry()
	{
		var options = new BffCacheOptions();
		options.AddCacheable("key", TimeSpan.FromSeconds(60));
		options.AddNeverCached("key");

		options.IsCacheable("key").Should().BeFalse();
	}

	#endregion
}
