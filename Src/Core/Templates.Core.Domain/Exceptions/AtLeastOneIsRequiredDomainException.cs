namespace Templates.Core.Domain.Exceptions;

public class AtLeastOneIsRequiredDomainException : DomainException
{
	public AtLeastOneIsRequiredDomainException(string parameterName, string message) : base(parameterName, message)
	{
	}

	public AtLeastOneIsRequiredDomainException(string parameterName) : this(parameterName, Resource.AtLeastOneIsRequired)
	{
	}
}
