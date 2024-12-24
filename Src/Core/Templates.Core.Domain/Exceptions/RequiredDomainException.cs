namespace Templates.Core.Domain.Exceptions;

public class RequiredDomainException : DomainException
{
	public RequiredDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public RequiredDomainException(string parameterName) : this(parameterName, Resource.Required)
	{

	}
}
