namespace Pivot.Framework.Authentication.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral token revocation abstraction.
/// </summary>
public interface ITokenRevocationService
{
	/// <summary>
	/// Revokes a token at the identity provider.
	/// </summary>
	Task RevokeTokenAsync(string token, string? tokenTypeHint = null, CancellationToken ct = default);
}