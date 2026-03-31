using Pivot.Framework.Domain;

namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Domain exception thrown when a value is outside an expected range.
/// </summary>
public class OutOfRangeDomainException : DomainException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="OutOfRangeDomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	public OutOfRangeDomainException(string parameterName, string message)
		: base(parameterName, message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OutOfRangeDomainException"/> class using the default resource message.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	public OutOfRangeDomainException(string parameterName)
		: this(parameterName, Resource.OutOfRange)
	{
	}

	#endregion
}
