using Microsoft.Extensions.Configuration;
using Pivot.Framework.Authentication.Storage;
using Pivot.Framework.Authentication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pivot.Framework.Authentication.AspNetCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Registers provider-neutral backend auth services backed by Keycloak.
/// </summary>
public static class KeycloakIdentityProviderExtensions
{
    #region Public Methods
    /// <summary>
    /// Registers the in-memory auth session store.
    /// </summary>
    public static IServiceCollection AddInMemoryAuthSessions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IAuthSessionStore, InMemoryAuthSessionStore>();
        return services;
    }
    
    /// <summary>
    /// Registers Keycloak-backed backend auth services and admin integrations.
    /// </summary>
    public static IServiceCollection AddKeycloakIdentityProviderServices(this IServiceCollection services, IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

		services.AddHttpClient<IIdentityProviderAuthService, KeycloakIdentityProviderAuthService>();
		services.AddHttpClient<ITokenIntrospectionService, KeycloakTokenIntrospectionService>();
		services.AddHttpClient<ITokenRevocationService, KeycloakTokenRevocationService>();
		services.AddHttpClient<IIdentityProviderAdminService, KeycloakIdentityProviderAdminService>();

		return services;
	}
	#endregion
}