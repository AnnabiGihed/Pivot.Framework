namespace Templates.Core.Domain.Exceptions;

public class AggregateDomainException : AggregateException
{
	public AggregateDomainException(IEnumerable<DomainException> exceptions) : base(exceptions)
	{ }
}
