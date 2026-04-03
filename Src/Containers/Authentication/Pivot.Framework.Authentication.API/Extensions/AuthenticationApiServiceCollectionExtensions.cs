using Microsoft.Extensions.DependencyInjection;

namespace Pivot.Framework.Authentication.API.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : DI helpers for the backend authentication API package.
/// </summary>
public static class AuthenticationApiServiceCollectionExtensions
{
	/// <summary>
	/// Registers common services used by the authentication API endpoints.
	/// </summary>
	public static IServiceCollection AddAuthenticationApi(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddEndpointsApiExplorer();
		return services;
	}
}
