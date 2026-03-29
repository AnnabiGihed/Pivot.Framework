using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Infrastructure.Abstraction.ServiceClients;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Base interface for typed HTTP service clients used in inter-service communication.
///              Each downstream service gets a typed client that handles authentication token
///              forwarding, resilience policies (retry + circuit breaker), and response mapping.
/// </summary>
public interface IServiceClient
{
	/// <summary>The name of the downstream service this client communicates with.</summary>
	string ServiceName { get; }
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration for a typed service client including base URL and resilience settings.
/// </summary>
public sealed class ServiceClientOptions
{
	/// <summary>Base URL of the downstream service.</summary>
	public required string BaseUrl { get; set; }

	/// <summary>Number of retry attempts for transient failures. Defaults to 3.</summary>
	public int RetryCount { get; set; } = 3;

	/// <summary>Initial retry delay in seconds (exponential backoff). Defaults to 1.</summary>
	public int RetryBaseDelaySeconds { get; set; } = 1;

	/// <summary>Circuit breaker: number of consecutive failures before opening. Defaults to 5.</summary>
	public int CircuitBreakerThreshold { get; set; } = 5;

	/// <summary>Circuit breaker: duration the circuit stays open in seconds. Defaults to 30.</summary>
	public int CircuitBreakerDurationSeconds { get; set; } = 30;

	/// <summary>Request timeout in seconds. Defaults to 30.</summary>
	public int TimeoutSeconds { get; set; } = 30;
}
