using System.Collections.ObjectModel;

namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Represents an aggregation of multiple <see cref="DomainException"/> instances.
///              Useful when validating multiple parameters and reporting all failures at once.
/// </summary>
public class AggregateDomainException : AggregateException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="AggregateDomainException"/> class.
	/// </summary>
	/// <param name="exceptions">The domain exceptions to aggregate.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="exceptions"/> is null.</exception>
	public AggregateDomainException(IEnumerable<DomainException> exceptions)
		: this(exceptions?.ToList() ?? throw new ArgumentNullException(nameof(exceptions)))
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="AggregateDomainException"/> class
	/// from a pre-materialized list of domain exceptions.
	/// </summary>
	/// <param name="materializedExceptions">The materialized list of domain exceptions.</param>
	private AggregateDomainException(List<DomainException> materializedExceptions)
		: base(BuildMessage(materializedExceptions), materializedExceptions)
	{
		DomainExceptions = new ReadOnlyCollection<DomainException>(materializedExceptions);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the aggregated domain exceptions as a strongly-typed read-only collection.
	/// </summary>
	public IReadOnlyCollection<DomainException> DomainExceptions { get; }

	#endregion

	#region Private Helpers

	/// <summary>
	/// Builds a summary message from the list of domain exceptions.
	/// </summary>
	private static string BuildMessage(List<DomainException> exceptions)
	{
		return exceptions.Count switch
		{
			0 => "One or more domain errors occurred.",
			1 => exceptions[0].Message,
			_ => $"{exceptions.Count} domain errors occurred."
		};
	}

	#endregion
}
