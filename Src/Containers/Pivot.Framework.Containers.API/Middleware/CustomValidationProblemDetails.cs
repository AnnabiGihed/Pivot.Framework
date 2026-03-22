using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.API.Middleware;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Specialized <see cref="ProblemDetails"/> for validation failures.
///              Adds a typed collection of validation errors.
/// </summary>
public sealed class CustomValidationProblemDetails : ProblemDetails
{
	#region Properties

	/// <summary>
	/// Collection of domain / validation errors associated with the problem.
	/// </summary>
	public IReadOnlyCollection<Error> ValidationErrors { get; init; } = Array.Empty<Error>();

	#endregion
}
