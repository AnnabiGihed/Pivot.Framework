namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Represents the provider-generated authorization URL returned to the caller.
/// </summary>
public sealed class AuthAuthorizationResult
{
	/// <summary>
	/// The fully qualified authorization URL.
	/// </summary>
	public string AuthorizationUrl { get; set; } = string.Empty;
}
