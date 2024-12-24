using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Exceptions;

public class ValidationException : Exception
{
	public ValidationException(Error message) : base(message)
	{
		ValidationErrors = new Error[] { message };
	}

	public ValidationException(Error message, IValidationResult validationResult) : base(message)
	{
		ValidationErrors = validationResult.Errors;
	}

	public Error[] ValidationErrors { get; set; }
}
