namespace Templates.Core.Domain.Exceptions;

public abstract class DomainException : ArgumentException
{
	private readonly string _message;

	public DomainException(string parameterName, string message) : base(message, parameterName)
	{
		ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
		_message = message ?? throw new ArgumentNullException(nameof(message));
	}
	public DomainException(string parameterName, string message, Exception innerException) : base(message, innerException)
	{
		ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
		_message = message ?? throw new ArgumentNullException(nameof(message));
	}

	public override string Message => _message;

	public string ParameterName { get; }
}
