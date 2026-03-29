using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pivot.Framework.Containers.API.RealTime.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering SignalR real-time push infrastructure.
///              Registers the EventPushService and SignalR hub for pushing real-time
///              event notifications to connected Blazor Server circuits.
/// </summary>
public static class SignalRExtensions
{
	/// <summary>
	/// Registers SignalR services and the EventPushService for real-time event notifications.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddPivotSignalR(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSignalR();
		services.TryAddSingleton<EventPushService>();

		return services;
	}
}
