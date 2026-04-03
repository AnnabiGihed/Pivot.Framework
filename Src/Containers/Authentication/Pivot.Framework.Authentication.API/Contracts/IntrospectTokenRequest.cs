namespace Pivot.Framework.Authentication.API.Contracts;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Request contract for token introspection endpoints.
/// </summary>
public sealed class IntrospectTokenRequest
{
	/// <summary>
	/// Token to introspect.
	/// </summary>
	public string Token { get; set; } = string.Empty;
}
