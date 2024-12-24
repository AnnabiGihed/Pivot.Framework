using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Exceptions;

public class BadRequestException : Exception
{
	public BadRequestException(Error message) : base(message)
	{
		ValidationErrors = new Error[] { message };
	}

	public BadRequestException(Error message, IValidationResult validationResult) : base(message)
	{
		ValidationErrors = validationResult.Errors;
	}

	public Error[] ValidationErrors { get; set; }
}
