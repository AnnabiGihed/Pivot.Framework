using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Authentication.Caching.Extensions;
using Pivot.Framework.Authentication.AspNetCore.Extensions;


namespace Pivot.Framework.Containers.API.ServiceInstallers;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Reusable service installer for ASP.NET Core APIs that need Keycloak JWT,
///              Redis token caching, and Swagger OAuth2 PKCE in one call.
///              Pass the Swagger document title and version as parameters so this
///              installer remains application-agnostic.
/// </summary>
public static class AddKeycloakAuthenticationServiceInstaller
{
	/// <summary>
	/// Registers the full Keycloak authentication stack:
	/// <list type="bullet">
	///   <item>JWT bearer authentication with Keycloak claims transformation</item>
	///   <item><c>ICurrentUser</c> scoped service + <c>IHttpContextAccessor</c></item>
	///   <item>Redis token caching (IDistributedTokenCache, ITokenRevocationCache, KeycloakRedisJwtEvents)</item>
	///   <item>Swagger / OpenAPI with Keycloak OAuth2 PKCE security definition</item>
	/// </list>
	/// </summary>
	/// <param name="services">The service collection to register the authentication stack into.</param>
	/// <param name="configuration">The application configuration containing the Keycloak and Redis sections.</param>
	/// <param name="swaggerTitle">The title to display in the Swagger UI (e.g. "My API").</param>
	/// <param name="swaggerVersion">The Swagger document version (e.g. "v1"). Defaults to "v1".</param>
	/// <returns>The updated service collection.</returns>
	public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration, string swaggerTitle, string swaggerVersion = "v1")
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);
		ArgumentException.ThrowIfNullOrWhiteSpace(swaggerTitle);
		ArgumentException.ThrowIfNullOrWhiteSpace(swaggerVersion);

		services.AddKeycloakAuthentication(configuration, o => o
			.WithCurrentUser()
			.WithRedisTokenCaching()
			.WithSwagger(swaggerTitle, swaggerVersion));

		return services;
	}
}