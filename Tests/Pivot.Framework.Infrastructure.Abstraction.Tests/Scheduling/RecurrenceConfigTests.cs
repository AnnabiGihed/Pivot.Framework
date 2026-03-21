using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Configurations;
using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Enums;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Scheduling;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="RecurrenceConfig"/>.
///              Verifies Cron expression generation for all recurrence types and intervals,
///              and exception handling for unsupported types.
/// </summary>
public class RecurrenceConfigTests
{
	#region Hourly Tests
	/// <summary>
	/// Verifies hourly recurrence with interval 1 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Hourly_Interval1_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Hourly, Interval = 1 };

		config.ToCronExpression().Should().Be("0 */1 * * *");
	}

	/// <summary>
	/// Verifies hourly recurrence with interval 6 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Hourly_Interval6_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Hourly, Interval = 6 };

		config.ToCronExpression().Should().Be("0 */6 * * *");
	}
	#endregion

	#region Daily Tests
	/// <summary>
	/// Verifies daily recurrence with interval 1 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Daily_Interval1_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Daily, Interval = 1 };

		config.ToCronExpression().Should().Be("0 0 */1 * *");
	}

	/// <summary>
	/// Verifies daily recurrence with interval 3 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Daily_Interval3_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Daily, Interval = 3 };

		config.ToCronExpression().Should().Be("0 0 */3 * *");
	}
	#endregion

	#region Weekly Tests
	/// <summary>
	/// Verifies weekly recurrence with interval 1 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Weekly_Interval1_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Weekly, Interval = 1 };

		config.ToCronExpression().Should().Be("0 0 * * 0/1");
	}

	/// <summary>
	/// Verifies weekly recurrence with interval 2 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Weekly_Interval2_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Weekly, Interval = 2 };

		config.ToCronExpression().Should().Be("0 0 * * 0/2");
	}
	#endregion

	#region Monthly Tests
	/// <summary>
	/// Verifies monthly recurrence with interval 1 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Monthly_Interval1_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Monthly, Interval = 1 };

		config.ToCronExpression().Should().Be("0 0 1 */1 *");
	}
	#endregion

	#region Yearly Tests
	/// <summary>
	/// Verifies yearly recurrence with interval 1 produces correct Cron expression.
	/// </summary>
	[Fact]
	public void ToCronExpression_Yearly_Interval1_ShouldReturnCorrectCron()
	{
		var config = new RecurrenceConfig { Type = RecurrenceType.Yearly, Interval = 1 };

		config.ToCronExpression().Should().Be("0 0 1 1 */1");
	}
	#endregion

	#region Unsupported Type Tests
	/// <summary>
	/// Verifies that an unsupported recurrence type throws <see cref="ArgumentOutOfRangeException"/>.
	/// </summary>
	[Fact]
	public void ToCronExpression_UnsupportedType_ShouldThrow()
	{
		var config = new RecurrenceConfig { Type = (RecurrenceType)999, Interval = 1 };

		var act = () => config.ToCronExpression();

		act.Should().Throw<ArgumentOutOfRangeException>();
	}
	#endregion

	#region Property Tests
	/// <summary>
	/// Verifies that properties can be set and retrieved.
	/// </summary>
	[Fact]
	public void Properties_ShouldBeSettableAndGettable()
	{
		var config = new RecurrenceConfig
		{
			Type = RecurrenceType.Daily,
			Interval = 5
		};

		config.Type.Should().Be(RecurrenceType.Daily);
		config.Interval.Should().Be(5);
	}
	#endregion
}
