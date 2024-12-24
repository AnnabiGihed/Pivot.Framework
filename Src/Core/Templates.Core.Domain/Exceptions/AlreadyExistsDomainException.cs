namespace Templates.Core.Domain.Exceptions;

public class AlreadyExistsDomainException : DomainException
{
	public AlreadyExistsDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public AlreadyExistsDomainException(string parameterName) : this(parameterName, Resource.AlreadyExists)
	{

	}
}
