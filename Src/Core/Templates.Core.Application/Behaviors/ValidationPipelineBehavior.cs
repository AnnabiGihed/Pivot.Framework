using MediatR;
using FluentValidation;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse>
   : IPipelineBehavior<TRequest, TResponse>
   where TRequest : IRequest<TResponse>
   where TResponse : Result
{
	/// <summary>
	/// Validators comming from the FluentValidation library.
	/// </summary>
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators) =>
		_validators = validators;

	/// <summary>
	/// What this method does:
	/// ----------------------
	/// +Validate the request object.
	/// +If any errors return a ValidationResult.
	/// +If no errors return the result of the next delegate execution.
	/// </summary>
	/// <param name="request">command or query</param>
	/// <param name="next"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// Skip validation if not validators present ...
		if (!_validators.Any())
		{
			// execute next in the requesthandler ...
			return await next();
		}

		// If we have validators:
		// + Perform the validation logic.
		// + Wrap result of the  validation logic into a ValidationResult object.
		// + Return the ValidationResult object from the pipeline.
		Error[] errors = _validators
			.Select(async validator => await validator.ValidateAsync(request))
			.SelectMany(validationResult => validationResult.Result.Errors)
			.Where(validationFailure => validationFailure is not null)
			.Select(failure => new Error(
				failure.PropertyName,
				failure.ErrorMessage))
			.Distinct()
			.ToArray();

		if (errors.Any())
		{
			return CreateValidationResult<TResponse>(errors);
		}

		return await next();
	}

	/// <summary>
	/// Create a Result class containing all ValidationErrors.
	/// </summary>
	/// <typeparam name="TResult">result class</typeparam>
	/// <param name="errors">errors passed to result class validation errors</param>
	/// <returns></returns>
	private static TResult CreateValidationResult<TResult>(Error[] errors)
		where TResult : Result
	{
		if (typeof(TResult) == typeof(Result))
		{
			// return non-generic validation result.
			return (ValidationResult.WithErrors(errors) as TResult)!; // ! = null-forgiving operator, because we assume never having nulls ...
		}

		// in case of returning a generic result
		object validationResult = typeof(ValidationResult<>)
			.GetGenericTypeDefinition()
			.MakeGenericType(typeof(TResult).GenericTypeArguments[0]) // we only have 1 generic argument in the generic result class.
			.GetMethod(nameof(ValidationResult.WithErrors))!
			.Invoke(null, new object?[] { errors })!;

		return (TResult)validationResult;
	}
}
