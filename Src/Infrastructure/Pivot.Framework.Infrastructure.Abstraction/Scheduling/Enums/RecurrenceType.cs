namespace Pivot.Framework.Infrastructure.Abstraction.Scheduling.Enums;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Defines the types of recurrence supported for recurring jobs.
/// </summary>
public enum RecurrenceType
{
	/// <summary>
	/// The job recurs every specified number of hours.
	/// </summary>
	Hourly,

	/// <summary>
	/// The job recurs every specified number of days.
	/// </summary>
	Daily,

	/// <summary>
	/// The job recurs weekly.
	/// </summary>
	Weekly,

	/// <summary>
	/// The job recurs every specified number of months.
	/// </summary>
	Monthly,

	/// <summary>
	/// The job recurs yearly.
	/// </summary>
	Yearly
}
