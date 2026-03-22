namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Domain exception thrown when at least one value is required but none were provided.
/// </summary>
public sealed class AtLeastOneIsRequiredDomainException : DomainException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="AtLeastOneIsRequiredDomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	public AtLeastOneIsRequiredDomainException(string parameterName, string message)
		: base(parameterName, message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AtLeastOneIsRequiredDomainException"/> class using the default resource message.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	public AtLeastOneIsRequiredDomainException(string parameterName)
		: this(parameterName, Resource.AtLeastOneIsRequired)
	{
	}

	#endregion
}
