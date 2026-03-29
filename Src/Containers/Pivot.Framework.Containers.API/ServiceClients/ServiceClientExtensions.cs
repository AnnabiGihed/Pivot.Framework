using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Pivot.Framework.Infrastructure.Abstraction.ServiceClients;

namespace Pivot.Framework.Containers.API.ServiceClients;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering typed HTTP service clients with
///              Polly resilience policies (retry with exponential backoff + circuit breaker).
///              Used for inter-service communication in the MDM platform.
/// </summary>
public static class ServiceClientExtensions
{
	/// <summary>
	/// Registers a typed HTTP service client with resilience policies.
	/// Configures IHttpClientFactory with retry (exponential backoff) and circuit breaker.
	/// </summary>
	/// <typeparam name="TClient">The typed client interface.</typeparam>
	/// <typeparam name="TImplementation">The typed client implementation.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">Action to configure the service client options.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddServiceClient<TClient, TImplementation>(
		this IServiceCollection services,
		Action<ServiceClientOptions> configure)
		where TClient : class
		where TImplementation : class, TClient
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configure);

		var options = new ServiceClientOptions { BaseUrl = string.Empty };
		configure(options);

		services.AddHttpClient<TClient, TImplementation>(client =>
		{
			client.BaseAddress = new Uri(options.BaseUrl);
			client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
		})
		.AddPolicyHandler(GetRetryPolicy(options))
		.AddPolicyHandler(GetCircuitBreakerPolicy(options));

		return services;
	}

	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ServiceClientOptions options)
	{
		return HttpPolicyExtensions
			.HandleTransientHttpError()
			.WaitAndRetryAsync(
				options.RetryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(options.RetryBaseDelaySeconds, retryAttempt)));
	}

	private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ServiceClientOptions options)
	{
		return HttpPolicyExtensions
			.HandleTransientHttpError()
			.CircuitBreakerAsync(
				options.CircuitBreakerThreshold,
				TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds));
	}
}
