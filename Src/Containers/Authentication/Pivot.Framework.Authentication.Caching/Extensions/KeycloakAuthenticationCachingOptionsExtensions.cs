using Pivot.Framework.Authentication.AspNetCore.Options;

namespace Pivot.Framework.Authentication.Caching.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Extends <see cref="KeycloakAuthenticationOptions"/> with Redis token caching support.
///              Kept in <c>Pivot.Framework.Authentication.Caching</c> so that the core
///              <c>Pivot.Framework.Authentication.AspNetCore</c> package has no compile-time
///              dependency on Redis — if you don't install this package, the option simply
///              doesn't exist.
/// </summary>
public static class KeycloakAuthenticationCachingOptionsExtensions
{
	/// <summary>
	/// Activates the Redis-backed token caching stack:
	/// <list type="bullet">
	///   <item>Redis <c>ICacheService</c></item>
	///   <item><c>IDistributedTokenCache</c> and <c>ITokenRevocationCache</c></item>
	///   <item><c>KeycloakRedisJwtEvents</c> wired into the JWT bearer pipeline</item>
	/// </list>
	/// Requires a valid <c>Redis</c> section in application configuration.
	/// </summary>
	public static KeycloakAuthenticationOptions WithRedisTokenCaching(
		this KeycloakAuthenticationOptions options)
	{
		options.AddRegistration(static (services, configuration) =>
		{
			services.AddKeycloakAuthenticationCaching(configuration);
		});

		return options;
	}
}