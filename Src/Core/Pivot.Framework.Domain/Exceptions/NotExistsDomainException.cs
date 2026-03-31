using Pivot.Framework.Domain;

namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Domain exception thrown when an expected entity/value does not exist.
/// </summary>
public class NotExistsDomainException : DomainException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="NotExistsDomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	public NotExistsDomainException(string parameterName, string message)
		: base(parameterName, message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NotExistsDomainException"/> class using the default resource message.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	public NotExistsDomainException(string parameterName)
		: this(parameterName, Resource.NotExists)
	{
	}

	#endregion
}
