namespace Templates.Core.Domain.Exceptions;

public class DomainExceptionScope : IDisposable
{
	List<DomainException> _domainExceptions = new();

	public void AddException(DomainException domainException)
	{
		_domainExceptions.Add(domainException);
	}

	public void ThrowIfAny()
	{
		if (_domainExceptions.Count > 0)
		{
			var aggregateException = new AggregateDomainException(_domainExceptions);
			Clear();
			throw aggregateException;
		}
	}

	private void Clear()
	{
		_domainExceptions = new List<DomainException>();
	}

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
	public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
	{
		ThrowIfAny();
	}
}
