namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Domain exception thrown when an unexpected/unknown domain error occurs.
/// </summary>
public class UnknownDomainException : DomainException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="UnknownDomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	public UnknownDomainException(string parameterName, string message)
		: base(parameterName, message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UnknownDomainException"/> class using the default resource message.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	public UnknownDomainException(string parameterName)
		: this(parameterName, Resource.Unknown)
	{
	}

	#endregion
}
