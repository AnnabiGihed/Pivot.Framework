using Polly.Retry;
using Polly.CircuitBreaker;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.Resilience;

/// <summary>
/// Wraps the Polly retry and circuit breaker policies used by the messaging infrastructure.
/// Registered as a singleton to avoid bare policy types in the DI container.
/// </summary>
public class MessagingResiliencePolicies
{
	public AsyncRetryPolicy RetryPolicy { get; }
	public AsyncCircuitBreakerPolicy CircuitBreakerPolicy { get; }

	public MessagingResiliencePolicies(AsyncRetryPolicy retryPolicy, AsyncCircuitBreakerPolicy circuitBreakerPolicy)
	{
		RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		CircuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
	}
}
