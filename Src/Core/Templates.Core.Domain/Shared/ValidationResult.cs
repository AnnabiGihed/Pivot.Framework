namespace Templates.Core.Domain.Shared;

public sealed class ValidationResult : Result, IValidationResult
{
	private ValidationResult(Error[] errors, ResultExceptionType resultExceptionType = default!)
		: base(false, IValidationResult.ValidationError, resultExceptionType) =>
		Errors = errors;

	public Error[] Errors { get; }

	public static ValidationResult WithErrors(Error[] errors) => new(errors);
}
