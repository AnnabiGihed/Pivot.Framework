using FluentAssertions;
using Pivot.Framework.Application.Abstractions.Correlation;

namespace Pivot.Framework.Application.Tests.Correlation;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="CorrelationContext"/>.
///              Verifies ambient correlation ID management via AsyncLocal.
/// </summary>
public class CorrelationContextTests
{
	#region Setup

	public CorrelationContextTests()
	{
		// Reset state before each test
		CorrelationContext.CorrelationId = null;
	}

	#endregion

	#region CorrelationId Tests

	/// <summary>
	/// Verifies that CorrelationId is null by default.
	/// </summary>
	[Fact]
	public void CorrelationId_ShouldDefaultToNull()
	{
		CorrelationContext.CorrelationId.Should().BeNull();
	}

	/// <summary>
	/// Verifies that CorrelationId can be set and retrieved.
	/// </summary>
	[Fact]
	public void CorrelationId_SetValue_ShouldBeRetrievable()
	{
		CorrelationContext.CorrelationId = "test-corr-123";

		CorrelationContext.CorrelationId.Should().Be("test-corr-123");
	}

	/// <summary>
	/// Verifies that CorrelationId can be reset to null.
	/// </summary>
	[Fact]
	public void CorrelationId_SetToNull_ShouldClear()
	{
		CorrelationContext.CorrelationId = "test-value";
		CorrelationContext.CorrelationId = null;

		CorrelationContext.CorrelationId.Should().BeNull();
	}

	#endregion

	#region EnsureCorrelationId Tests

	/// <summary>
	/// Verifies that EnsureCorrelationId generates a new ID when none exists.
	/// </summary>
	[Fact]
	public void EnsureCorrelationId_WhenNull_ShouldGenerateNewId()
	{
		var result = CorrelationContext.EnsureCorrelationId();

		result.Should().NotBeNullOrEmpty();
		Guid.TryParse(result, out _).Should().BeTrue();
		CorrelationContext.CorrelationId.Should().Be(result);
	}

	/// <summary>
	/// Verifies that EnsureCorrelationId returns the existing ID when one is set.
	/// </summary>
	[Fact]
	public void EnsureCorrelationId_WhenAlreadySet_ShouldReturnExistingId()
	{
		CorrelationContext.CorrelationId = "existing-id";

		var result = CorrelationContext.EnsureCorrelationId();

		result.Should().Be("existing-id");
	}

	/// <summary>
	/// Verifies that EnsureCorrelationId is idempotent when called multiple times.
	/// </summary>
	[Fact]
	public void EnsureCorrelationId_CalledMultipleTimes_ShouldReturnSameId()
	{
		var first = CorrelationContext.EnsureCorrelationId();
		var second = CorrelationContext.EnsureCorrelationId();

		first.Should().Be(second);
	}

	#endregion

	#region Async Flow Tests

	/// <summary>
	/// Verifies that CorrelationId flows across async boundaries.
	/// </summary>
	[Fact]
	public async Task CorrelationId_ShouldFlowAcrossAsyncBoundaries()
	{
		CorrelationContext.CorrelationId = "async-flow-test";

		var capturedId = await Task.Run(() => CorrelationContext.CorrelationId);

		capturedId.Should().Be("async-flow-test");
	}

	#endregion
}
