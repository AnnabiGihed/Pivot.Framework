using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pivot.Framework.Authentication.AspNetCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : INTERNAL — kept for intra-package use only.
///              Replaced publicly by <see cref="KeycloakAuthenticationExtensions.AddKeycloakAuthentication"/>
///              with <c>.WithCurrentUser()</c>.
///              Do not call from application code.
/// </summary>
internal static class KeycloakServiceCollectionExtensions
{
	/// <summary>
	/// Registers Keycloak JWT + ICurrentUser + IHttpContextAccessor.
	/// Use <c>services.AddKeycloakAuthentication(config, o => o.WithCurrentUser())</c> instead.
	/// </summary>
	internal static IServiceCollection AddKeycloakBackend(this IServiceCollection services, IConfiguration configuration)
	{
		services.RegisterCurrentUser();
		services.RegisterCoreJwtBearer(configuration);
		return services;
	}
}