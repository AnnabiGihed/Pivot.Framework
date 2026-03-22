using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Scheduling.Configurations;

namespace Pivot.Framework.Infrastructure.Abstraction.Scheduling.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Interface for managing recurring jobs with the underlying scheduler. Supports generic identifiers,
///              job results, and parameterized job functions.
/// </summary>
/// <typeparam name="TIdentifier">The type of the unique identifier for jobs.</typeparam>
/// <typeparam name="TParams">The type of the parameters for the job function.</typeparam>
/// <typeparam name="TValue">The return type of the job function result.</typeparam>
public interface IRecurringJobService<TIdentifier, TParams, TValue>
{
	#region Methods

	/// <summary>
	/// Deletes an existing recurring job.
	/// </summary>
	/// <param name="identifier">The unique identifier of the job to delete.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the delete operation.</returns>
	Result DeleteJob(TIdentifier identifier);

	/// <summary>
	/// Executes a recurring job immediately.
	/// </summary>
	/// <param name="identifier">The unique identifier of the job to execute.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the execution.</returns>
	Result RunJobNow(TIdentifier identifier);

	/// <summary>
	/// Creates a new recurring job or updates an existing one, with or without parameters.
	/// </summary>
	/// <param name="identifier">The unique identifier for the job.</param>
	/// <param name="config">The recurrence configuration defining the schedule.</param>
	/// <param name="jobFunction">The asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the creation.</returns>
	Result CreateJob(TIdentifier identifier,	RecurrenceConfig config, Func<Task<Result<TValue>>> jobFunction);

	/// <summary>
	/// Creates or updates a recurring job with parameters.
	/// </summary>
	/// <param name="identifier">The unique identifier for the job.</param>
	/// <param name="config">The recurrence configuration defining the schedule.</param>
	/// <param name="parameters">The parameters to pass to the job function.</param>
	/// <param name="jobFunction">The asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the creation.</returns>
	Result CreateJobWithParams(TIdentifier identifier, RecurrenceConfig config, TParams parameters, Func<TParams, Task<Result<TValue>>> jobFunction);

	/// <summary>
	/// Modifies an existing recurring job's schedule and parameters.
	/// </summary>
	/// <param name="identifier">The unique identifier of the job to modify.</param>
	/// <param name="config">The updated recurrence configuration.</param>
	/// <param name="parameters">The updated parameters to pass to the job function.</param>
	/// <param name="jobFunction">The updated asynchronous function to execute on each recurrence.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the modification.</returns>
	Result ModifyJobWithParams(TIdentifier identifier, RecurrenceConfig config, TParams parameters, Func<TParams, Task<Result<TValue>>> jobFunction);

	#endregion
}
