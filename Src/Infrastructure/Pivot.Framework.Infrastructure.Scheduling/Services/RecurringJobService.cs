using Hangfire;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Services;
using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Configurations;

namespace Pivot.Framework.Infrastructure.Scheduling.Services;

/// <summary>
/// Implementation of the IRecurringJobManager interface for managing Hangfire recurring jobs.
/// Supports parameterized job functions and operations like create, modify, delete, and run now.
/// </summary>
/// <typeparam name="TIdentifier">The type of the unique identifier for jobs.</typeparam>
/// <typeparam name="TParams">The type of the parameters for the job function.</typeparam>
/// <typeparam name="TValue">The return type of the job function result.</typeparam>
public class RecurringJobService<TIdentifier, TParams, TValue> : IRecurringJobService<TIdentifier, TParams, TValue>
{
	#region Public Methods

	/// <summary>
	/// Triggers a recurring job to execute immediately.
	/// </summary>
	/// <param name="identifier">The unique identifier of the job.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public Result RunJobNow(TIdentifier identifier)
	{
		try
		{
			var jobId = ConvertIdentifierToString(identifier);

			BackgroundJob.Enqueue(() => RecurringJob.TriggerJob(jobId));

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure<TValue>(new Error("Failed to run job immediately.", ex.Message));
		}
	}

	/// <summary>
	/// Removes a recurring job if it exists.
	/// </summary>
	/// <param name="identifier">The unique identifier of the job to delete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public Result DeleteJob(TIdentifier identifier)
	{
		try
		{
			var jobId = ConvertIdentifierToString(identifier);

			RecurringJob.RemoveIfExists(jobId);

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("Failed to delete job.", ex.Message));
		}
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Converts the job identifier to its string representation.
	/// </summary>
	/// <param name="identifier">The identifier to convert.</param>
	/// <returns>The string representation of the identifier.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="identifier"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the identifier cannot be converted to string.</exception>
	protected string ConvertIdentifierToString(TIdentifier identifier)
	{
		if (identifier == null)
			throw new ArgumentNullException(nameof(identifier));

		return identifier.ToString() ?? throw new InvalidOperationException("Identifier cannot be converted to a string.");
	}

	#endregion

	#region IRecurringJobService Implementation

	/// <summary>
	/// Creates or updates a recurring job with the specified schedule and function.
	/// </summary>
	/// <param name="identifier">The unique identifier for the job.</param>
	/// <param name="config">The recurrence configuration defining the schedule.</param>
	/// <param name="jobFunction">The asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public Result CreateJob(TIdentifier identifier, RecurrenceConfig config, Func<Task<Result<TValue>>> jobFunction)
	{
		try
		{
			var jobId = ConvertIdentifierToString(identifier);

			RecurringJob.AddOrUpdate(jobId, () => jobFunction(), config.ToCronExpression());

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("Failed to create job.", ex.Message));
		}
	}

	/// <summary>
	/// Creates or updates a recurring job with parameters.
	/// </summary>
	/// <param name="identifier">The unique identifier for the job.</param>
	/// <param name="config">The recurrence configuration defining the schedule.</param>
	/// <param name="parameters">The parameters to pass to the job function.</param>
	/// <param name="jobFunction">The asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public Result CreateJobWithParams(TIdentifier identifier, RecurrenceConfig config, TParams parameters, Func<TParams, Task<Result<TValue>>> jobFunction)
	{
		try
		{
			var jobId = ConvertIdentifierToString(identifier);

			RecurringJob.AddOrUpdate(jobId, () => jobFunction(parameters), config.ToCronExpression());

			return Result.Success();
		}
		catch (Exception ex)
		{
			return Result.Failure(new Error("Failed to create job with parameters.", ex.Message));
		}
	}

	/// <summary>
	/// Modifies an existing recurring job by replacing its schedule and function.
	/// Delegates to <see cref="CreateJobWithParams"/> (Hangfire's AddOrUpdate is idempotent).
	/// </summary>
	/// <param name="identifier">The unique identifier of the job to modify.</param>
	/// <param name="config">The updated recurrence configuration.</param>
	/// <param name="parameters">The updated parameters to pass to the job function.</param>
	/// <param name="jobFunction">The updated asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure.</returns>
	public Result ModifyJobWithParams(TIdentifier identifier, RecurrenceConfig config, TParams parameters, Func<TParams, Task<Result<TValue>>> jobFunction)
	{
		return CreateJobWithParams(identifier, config, parameters, jobFunction);
	}

	#endregion
}
