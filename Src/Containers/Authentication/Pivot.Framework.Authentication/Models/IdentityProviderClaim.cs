namespace Pivot.Framework.Authentication.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Provider-neutral identity claim model.
/// </summary>
public sealed class IdentityProviderClaim
{
	#region Properties
	/// <summary>
	/// Claim type/name.
	/// </summary>
	public string Type { get; set; } = string.Empty;

	/// <summary>
	/// Claim value.
	/// </summary>
	public string Value { get; set; } = string.Empty;
	#endregion
}