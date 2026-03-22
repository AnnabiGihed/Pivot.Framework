namespace Pivot.Framework.Application.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Provides the identity of the current user for audit stamping.
///              Implement this interface to decouple audit operations from ASP.NET Core HTTP context.
/// </summary>
public interface ICurrentUserProvider
{
	/// <summary>
	/// Returns the identifier (e.g., username or user ID) of the currently authenticated user.
	/// </summary>
	string GetCurrentUser();
}
