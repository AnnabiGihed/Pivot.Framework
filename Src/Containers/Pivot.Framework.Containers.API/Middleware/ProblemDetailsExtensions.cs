using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Helpers to enrich <see cref="ProblemDetails"/> with validation errors.
/// </summary>
public static class ProblemDetailsExtensions
{
	#region Public Methods

	/// <summary>
	/// Attaches a collection of validation errors to the <see cref="ProblemDetails.Extensions"/> dictionary.
	/// </summary>
	/// <param name="problemDetails">The problem details instance to enrich.</param>
	/// <param name="errors">The validation errors to attach.</param>
	/// <returns>The same <see cref="ProblemDetails"/> instance for fluent chaining.</returns>
	public static ProblemDetails WithValidationErrors(
		this ProblemDetails problemDetails,
		IReadOnlyCollection<Error> errors)
	{
		ArgumentNullException.ThrowIfNull(problemDetails);
		ArgumentNullException.ThrowIfNull(errors);

		if (errors.Count > 0)
			problemDetails.Extensions["validationErrors"] = errors;

		return problemDetails;
	}

	#endregion
}
