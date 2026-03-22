namespace Pivot.Framework.Domain.Exceptions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Changed base class from <see cref="ArgumentException"/> to <see cref="Exception"/>.
///              Domain validation failures are not argument errors; they represent business rule violations.
/// Purpose     : Base exception type for domain validation failures.
///              Carries the parameter name that triggered the violation as a first-class property.
/// </summary>
public abstract class DomainException : Exception
{
	#region Properties

	/// <summary>
	/// Gets the name of the parameter that caused the domain validation failure.
	/// </summary>
	public string ParameterName { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainException"/> class.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	protected DomainException(string parameterName, string message)
		: base(message)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
		ArgumentNullException.ThrowIfNull(message);
		ParameterName = parameterName;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DomainException"/> class with an inner exception.
	/// </summary>
	/// <param name="parameterName">The name of the parameter that caused the exception.</param>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception.</param>
	protected DomainException(string parameterName, string message, Exception innerException)
		: base(message, innerException)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(innerException);
		ParameterName = parameterName;
	}

	#endregion
}
