using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Responses;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Legacy command response DTO.
///              Kept for backward compatibility with consumers not yet migrated to Result-based responses.
/// </summary>
public sealed class BaseCommandResponse
{
	#region Properties
	/// <summary>
	/// Gets a value indicating whether the command executed successfully.
	/// </summary>
	public bool Success { get; }

	/// <summary>
	/// Gets a human-readable message describing the outcome.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Gets the collection of validation error messages, if any.
	/// </summary>
	public IReadOnlyCollection<string> ValidationErrors { get; }
	#endregion

	#region Constructors
	private BaseCommandResponse(bool success, string message, IEnumerable<string>? validationErrors = null)
	{
		Success = success;
		Message = message;
		ValidationErrors = validationErrors?.ToArray() ?? Array.Empty<string>();
	}
	#endregion

	#region Factory Methods
	/// <summary>
	/// Creates a successful command response with an optional message.
	/// </summary>
	/// <param name="message">An optional success message.</param>
	public static BaseCommandResponse Ok(string? message = null)
		=> new(true, message ?? string.Empty);

	/// <summary>
	/// Creates a failed command response with an error message and optional validation errors.
	/// </summary>
	/// <param name="message">The failure message.</param>
	/// <param name="errors">Optional collection of <see cref="Error"/> instances whose messages are extracted.</param>
	public static BaseCommandResponse Fail(
		string message,
		IEnumerable<Error>? errors = null)
	{
		var validationErrors = errors?
			.Where(e => e is not null && e != Error.None)
			.Select(e => e.Message)
			.ToArray();

		return new BaseCommandResponse(false, message, validationErrors);
	}
	#endregion
}
