using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Pivot.Framework.Containers.API.Observability;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for registering OpenTelemetry distributed tracing,
///              metrics collection, and structured logging. Configures OTLP exporters
///              for Prometheus/Grafana integration.
/// </summary>
public static class ObservabilityExtensions
{
	/// <summary>
	/// Adds OpenTelemetry tracing and metrics with OTLP exporter.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="serviceName">The logical name of this service for trace identification.</param>
	/// <param name="otlpEndpoint">The OTLP collector endpoint (e.g., "http://localhost:4317").</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddPivotObservability(
		this IServiceCollection services,
		string serviceName,
		string? otlpEndpoint = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

		services.AddOpenTelemetry()
			.WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddSource(serviceName);

				if (!string.IsNullOrEmpty(otlpEndpoint))
				{
					tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
				}
			})
			.WithMetrics(metrics =>
			{
				metrics
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddRuntimeInstrumentation();

				if (!string.IsNullOrEmpty(otlpEndpoint))
				{
					metrics.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
				}
			});

		return services;
	}
}
