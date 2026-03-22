namespace Pivot.Framework.Domain.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Standard validation-related errors.
/// </summary>
public static class ValidationErrors
{
	#region Static Instances

	/// <summary>
	/// Gets the default validation error indicating a validation problem occurred.
	/// </summary>
	public static readonly Error ValidationError =
		new("ValidationError", "A validation problem occurred.");

	#endregion
}
