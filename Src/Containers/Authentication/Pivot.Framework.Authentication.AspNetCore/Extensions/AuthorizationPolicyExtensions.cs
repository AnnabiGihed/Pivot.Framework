using Microsoft.AspNetCore.Authorization;

namespace Pivot.Framework.Authentication.AspNetCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Small helper extensions for registering common authorization policies.
/// </summary>
public static class AuthorizationPolicyExtensions
{
	#region Public Methods
	/// <summary>
	/// Adds a role-based authorization policy.
	/// </summary>
	public static AuthorizationOptions AddRolePolicy(this AuthorizationOptions options, string policyName, params string[] roles)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

		options.AddPolicy(policyName, policy => policy.RequireRole(roles));
		return options;
	}

	/// <summary>
	/// Adds a claim-based authorization policy.
	/// </summary>
	public static AuthorizationOptions AddClaimPolicy(this AuthorizationOptions options, string policyName, string claimType, params string[] allowedValues)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
		ArgumentException.ThrowIfNullOrWhiteSpace(claimType);

		options.AddPolicy(policyName, policy =>
		{
			if (allowedValues.Length == 0)
				policy.RequireClaim(claimType);
			else
				policy.RequireClaim(claimType, allowedValues);
		});

		return options;
	}

	/// <summary>
	/// Adds a scope-based authorization policy that checks either <c>scope</c> or <c>scp</c> claims.
	/// </summary>
	public static AuthorizationOptions AddScopePolicy(this AuthorizationOptions options, string policyName, params string[] scopes)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

		options.AddPolicy(policyName, policy => policy.RequireAssertion(context =>
		{
			var grantedScopes = context.User.Claims
				.Where(claim => claim.Type is "scope" or "scp")
				.SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			return scopes.All(grantedScopes.Contains);
		}));

		return options;
	}
	#endregion
}