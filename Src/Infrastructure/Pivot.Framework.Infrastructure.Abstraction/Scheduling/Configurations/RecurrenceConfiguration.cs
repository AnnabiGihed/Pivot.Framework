using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Enums;

namespace Pivot.Framework.Infrastructure.Abstraction.Scheduling.Configurations;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Represents the configuration for recurring jobs, including the type and interval of recurrence.
///              Provides conversion to standard Cron expressions compatible with the underlying scheduler.
/// </summary>
public class RecurrenceConfig
{
	#region Properties

	/// <summary>
	/// The type of recurrence (e.g., hourly, daily, weekly, etc.).
	/// </summary>
	public RecurrenceType Type { get; set; }

	/// <summary>
	/// The interval for the recurrence (e.g., every X hours or days).
	/// </summary>
	public int Interval { get; set; }

	#endregion

	#region Public Methods

	/// <summary>
	/// Converts the recurrence configuration into a standard Cron expression
	/// compatible with the underlying scheduler.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported recurrence type is used.</exception>
	public string ToCronExpression()
	{
		return Type switch
		{
			RecurrenceType.Hourly => $"0 */{Interval} * * *", // Every 'Interval' hours.
			RecurrenceType.Daily => $"0 0 */{Interval} * *", // Every 'Interval' days at midnight.
			RecurrenceType.Weekly => Interval == 1
				? "0 0 * * 0"  // Every Sunday
				: throw new NotSupportedException($"Weekly recurrence with interval > 1 is not supported in standard cron. Use a custom scheduler."),
			RecurrenceType.Monthly => $"0 0 1 */{Interval} *", // Every 'Interval' months on the 1st at midnight.
			RecurrenceType.Yearly => Interval == 1
				? "0 0 1 1 *"  // January 1st
				: throw new NotSupportedException($"Yearly recurrence with interval > 1 is not supported in standard cron. Use a custom scheduler."),
			_ => throw new ArgumentOutOfRangeException(nameof(Type), "Unsupported recurrence type.")
		};
	}

	#endregion
}
