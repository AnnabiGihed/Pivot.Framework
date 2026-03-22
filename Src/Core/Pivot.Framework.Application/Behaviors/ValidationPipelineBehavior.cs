using System.Collections.Concurrent;
using FluentValidation;
using MediatR;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Application.Behaviors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : MediatR pipeline behavior that executes FluentValidation validators for the request.
///              If validation errors exist, returns a validation failure Result (ValidationResult / ValidationResult{T}).
///              Otherwise continues to the next handler.
/// </summary>
/// <typeparam name="TRequest">The request type (command or query).</typeparam>
/// <typeparam name="TResponse">The response type, constrained to <see cref="Result"/>.</typeparam>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : Result
{
	#region Fields
	private static readonly ConcurrentDictionary<Type, Func<Error[], object>> _validationResultFactories = new();

	private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationPipelineBehavior{TRequest, TResponse}"/> class.
	/// </summary>
	/// <param name="validators">Validators registered for the request type.</param>
	public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators?.ToArray() ?? throw new ArgumentNullException(nameof(validators));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Executes FluentValidation and returns validation failures as a ValidationResult.
	/// </summary>
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(next);

		if (_validators.Count == 0)
			return await next();

		var context = new ValidationContext<TRequest>(request);

		// Run all validators concurrently and await properly.
		var validationResults = await Task.WhenAll(
			_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

		var errors = validationResults
			.SelectMany(r => r.Errors)
			.Where(f => f is not null)
			.Select(f => new Error(
				code: f.PropertyName,   // You can change this to a standard code if you prefer.
				message: f.ErrorMessage))
			// Note: Distinct uses Error.Equals which compares both Code and Message.
		// Errors with the same code but different messages (e.g., localized) will NOT be deduplicated.
		.Distinct()
			.ToArray();

		if (errors.Length > 0)
			return CreateValidationResult<TResponse>(errors);

		return await next();
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Creates either a non-generic <see cref="ValidationResult"/> (when TResponse is Result)
	/// or a generic <see cref="ValidationResult{T}"/> (when TResponse is Result{T}).
	/// Uses a cached factory delegate to avoid repeated reflection lookups.
	/// </summary>
	private static TResult CreateValidationResult<TResult>(Error[] errors)
		where TResult : Result
	{
		var factory = _validationResultFactories.GetOrAdd(typeof(TResult), type =>
		{
			if (type == typeof(Result))
				return errs => ValidationResult.WithErrors(errs);

			var genericType = type.GetGenericArguments()[0];
			var validationResultType = typeof(ValidationResult<>).MakeGenericType(genericType);
			var method = validationResultType.GetMethod(nameof(ValidationResult<int>.WithErrors), new[] { typeof(Error[]) })!;
			return errs => method.Invoke(null, new object[] { errs })!;
		});
		return (TResult)factory(errors);
	}
	#endregion
}
