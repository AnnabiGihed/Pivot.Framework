using Templates.Core.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Templates.Core.Authentication.AspNetCore.Extensions;

namespace Templates.Core.Containers.API.ServiceInstallers;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Wires Keycloak JWT + Redis token caching + Swagger OAuth into DI.
/// </summary>
public static class AddKeycloakAuthenticationServiceInstaller
{
	#region Public Methods
	/// <summary>
	/// Registers Keycloak authentication support, including:
	/// - Redis caching used for token-related operations
	/// - Swagger/OpenAPI configuration with Keycloak security definition and requirement
	/// </summary>
	/// <param name="services">The DI service collection.</param>
	/// <param name="configuration">Application configuration.</param>
	public static void AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddKeycloakRedisCache(configuration);

		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new() { Title = "Curvia API", Version = "v1" });

			// Pass IConfiguration directly to avoid calling services.BuildServiceProvider(),
			// which creates a second DI container root and causes an ASP.NET Core warning.
			c.AddKeycloakSecurityDefinition(configuration);
			c.AddKeycloakSecurityRequirement();
		});
	}
	#endregion
}