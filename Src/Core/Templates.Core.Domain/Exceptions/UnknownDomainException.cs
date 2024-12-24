namespace Templates.Core.Domain.Exceptions;

public class UnknownDomainException : DomainException
{
	public UnknownDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public UnknownDomainException(string parameterName) : this(parameterName, Resource.Unknown)
	{

	}
}
