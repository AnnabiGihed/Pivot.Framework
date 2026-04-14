using Pivot.Framework.Authentication.Models;

namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral token introspection abstraction.
/// </summary>
public interface ITokenIntrospectionService
{
	#region Methods
	/// <summary>
	/// Introspects a token and returns the provider response.
	/// </summary>
	Task<TokenIntrospectionResult> IntrospectTokenAsync(string token, CancellationToken ct = default);
	#endregion
}