namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Domain exception thrown when an entity/value already exists.
/// </summary>
public sealed class AlreadyExistsDomainException : DomainException
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="AlreadyExistsDomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	public AlreadyExistsDomainException(string parameterName, string message)
		: base(parameterName, message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AlreadyExistsDomainException"/> class using the default resource message.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	public AlreadyExistsDomainException(string parameterName)
		: this(parameterName, Resource.AlreadyExists)
	{
	}

	#endregion
}
