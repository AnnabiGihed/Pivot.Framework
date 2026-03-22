namespace Pivot.Framework.Domain.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Represents a domain/application error.
///              This is a lightweight immutable value object used in Result-based flows
///              instead of exceptions.
/// </summary>
public sealed class Error : IEquatable<Error>
{
	#region Static Instances

	/// <summary>
	/// Represents the absence of an error.
	/// </summary>
	public static readonly Error None = new(string.Empty, string.Empty);

	/// <summary>
	/// Represents a null value error.
	/// </summary>
	public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="Error"/> class.
	/// </summary>
	/// <param name="code">Unique error code.</param>
	/// <param name="message">Human-readable message.</param>
	public Error(string code, string message)
	{
		ArgumentNullException.ThrowIfNull(code);
		ArgumentNullException.ThrowIfNull(message);

		Code = code;
		Message = message;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the unique error code.
	/// </summary>
	public string Code { get; }

	/// <summary>
	/// Gets the human-readable error message.
	/// </summary>
	public string Message { get; }

	#endregion

	#region Operators

	/// <summary>
	/// Implicit conversion to string returns the error code.
	/// </summary>
	public static implicit operator string(Error error) => error.Code;

	/// <summary>
	/// Determines whether two <see cref="Error"/> instances are equal.
	/// </summary>
	public static bool operator ==(Error? a, Error? b)
	{
		if (ReferenceEquals(a, b))
			return true;

		if (a is null || b is null)
			return false;

		return a.Equals(b);
	}

	/// <summary>
	/// Determines whether two <see cref="Error"/> instances are not equal.
	/// </summary>
	public static bool operator !=(Error? a, Error? b) => !(a == b);

	#endregion

	#region Equality

	/// <summary>
	/// Determines equality based on code and message.
	/// </summary>
	public bool Equals(Error? other)
	{
		if (other is null)
			return false;

		return Code == other.Code && Message == other.Message;
	}

	/// <summary>
	/// Determines whether the specified object is equal to the current <see cref="Error"/>.
	/// </summary>
	public override bool Equals(object? obj) => obj is Error error && Equals(error);

	/// <summary>
	/// Returns a hash code based on the error code and message.
	/// </summary>
	public override int GetHashCode() => HashCode.Combine(Code, Message);

	#endregion

	#region Public Methods

	/// <summary>
	/// Returns the error code.
	/// </summary>
	public override string ToString() => Code;

	#endregion

	#region Predefined Errors

	/// <summary>
	/// Creates a system-level error.
	/// </summary>
	public static Error SystemError(string message)
		=> new("Error.SystemError", message);

	/// <summary>
	/// Creates an invalid value error.
	/// </summary>
	public static Error InvalidValue(string message)
		=> new("Error.InvalidValue", message);

	#endregion
}
