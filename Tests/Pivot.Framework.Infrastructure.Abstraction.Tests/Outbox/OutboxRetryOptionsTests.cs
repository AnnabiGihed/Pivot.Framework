using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Retry;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Outbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="OutboxRetryOptions"/>.
///              Verifies default values and property assignment.
/// </summary>
public class OutboxRetryOptionsTests
{
	#region Default Value Tests

	/// <summary>
	/// Verifies that MaxRetryCount defaults to 5.
	/// </summary>
	[Fact]
	public void MaxRetryCount_ShouldDefaultToFive()
	{
		var options = new OutboxRetryOptions();

		options.MaxRetryCount.Should().Be(5);
	}

	/// <summary>
	/// Verifies that EmitFailureEvent defaults to true.
	/// </summary>
	[Fact]
	public void EmitFailureEvent_ShouldDefaultToTrue()
	{
		var options = new OutboxRetryOptions();

		options.EmitFailureEvent.Should().BeTrue();
	}

	#endregion

	#region Property Assignment Tests

	/// <summary>
	/// Verifies that properties can be customized.
	/// </summary>
	[Fact]
	public void Properties_ShouldBeConfigurable()
	{
		var options = new OutboxRetryOptions
		{
			MaxRetryCount = 10,
			EmitFailureEvent = false
		};

		options.MaxRetryCount.Should().Be(10);
		options.EmitFailureEvent.Should().BeFalse();
	}

	#endregion
}
