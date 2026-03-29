using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.BFF.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering BFF infrastructure:
///              partial-failure response filter, cache service, and cache options.
/// </summary>
public static class BffExtensions
{
	/// <summary>
	/// Registers BFF infrastructure: in-memory cache service, response filter, and cache options.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">Optional delegate to configure BFF cache eligibility rules.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddBffInfrastructure(
		this IServiceCollection services,
		Action<BffCacheOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.Configure<BffCacheOptions>(options =>
		{
			configure?.Invoke(options);
		});

		services.TryAddSingleton<IBffCacheService, InMemoryBffCacheService>();

		return services;
	}
}
