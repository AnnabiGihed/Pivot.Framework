namespace Templates.Core.Domain.Shared;

public class Result
{
	protected internal Result(bool isSuccess, Error error, ResultExceptionType resultExceptionType = ResultExceptionType.BadRequest)
	{
		if (isSuccess && error != Error.None)
		{
			throw new InvalidOperationException();
		}

		if (!isSuccess && error == Error.None)
		{
			throw new InvalidOperationException();
		}

		ResultExceptionType = resultExceptionType;
		IsSuccess = isSuccess;
		Error = error;
	}

	public bool IsSuccess { get; }

	public bool IsFailure => !IsSuccess;

	public Error Error { get; }

	public ResultExceptionType ResultExceptionType { get; }

	public static Result Success() => new(true, Error.None, ResultExceptionType.None);

	public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None, ResultExceptionType.None);

	public static Result Failure(Error error, ResultExceptionType resultExceptionType = default!) => new(false, error, resultExceptionType);

	public static Result<TValue> Failure<TValue>(Error error, ResultExceptionType resultExceptionType = default!) => new(default, false, error, resultExceptionType);

	public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

}
