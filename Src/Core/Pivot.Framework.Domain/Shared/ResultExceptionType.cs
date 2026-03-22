namespace Pivot.Framework.Domain.Shared;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Classification used by application/API layers to map <see cref="Result"/> failures
///              to transport semantics (e.g., HTTP status codes).
/// </summary>
public enum ResultExceptionType
{
	/// <summary>
	/// No exception type. Used for successful results.
	/// </summary>
	None = 0,

	/// <summary>
	/// Client provided invalid or malformed data.
	/// </summary>
	ValidationError = 1,

	/// <summary>
	/// The requested resource was not found.
	/// </summary>
	NotFound = 2,

	/// <summary>
	/// A conflicting operation was detected (e.g., concurrency or duplicate).
	/// </summary>
	Conflict = 3,

	/// <summary>
	/// Authentication is required or has failed.
	/// </summary>
	AuthenticationRequired = 4,

	/// <summary>
	/// The caller does not have sufficient permissions.
	/// </summary>
	AccessDenied = 5
}
