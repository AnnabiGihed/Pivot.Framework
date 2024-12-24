namespace Templates.Core.Domain.Exceptions;

public class OutOfRangeDomainException : DomainException
{
	public OutOfRangeDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public OutOfRangeDomainException(string parameterName) : this(parameterName, Resource.OutOfRange)
	{

	}
}
