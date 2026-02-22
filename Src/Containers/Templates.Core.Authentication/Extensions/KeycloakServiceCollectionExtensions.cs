using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Templates.Core.Authentication.Models;

namespace Templates.Core.Authentication.Extensions;

/// <summary>
/// Convenience extensions that compose all backend Keycloak registrations.
/// </summary>
public static class KeycloakServiceCollectionExtensions
{
	/// <summary>
	/// Registers:
	/// - Keycloak JWT authentication (via <see cref="KeycloakAuthenticationExtensions"/>)
	/// - <see cref="ICurrentUser"/> scoped service
	/// - <see cref="IHttpContextAccessor"/>
	///
	/// Call this once from your API's <c>Program.cs</c>:
	/// <code>
	///   builder.Services.AddKeycloakBackend(builder.Configuration);
	/// </code>
	/// </summary>
	public static IServiceCollection AddKeycloakBackend(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUser, CurrentUser>();
		services.AddKeycloakAuthentication(configuration);

		return services;
	}
}