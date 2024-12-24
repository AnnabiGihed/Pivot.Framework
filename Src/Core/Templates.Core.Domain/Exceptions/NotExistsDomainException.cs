namespace Templates.Core.Domain.Exceptions;

public class NotExistsDomainException : DomainException
{
	public NotExistsDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public NotExistsDomainException(string parameterName) : this(parameterName, Resource.NotExists)
	{

	}
}
