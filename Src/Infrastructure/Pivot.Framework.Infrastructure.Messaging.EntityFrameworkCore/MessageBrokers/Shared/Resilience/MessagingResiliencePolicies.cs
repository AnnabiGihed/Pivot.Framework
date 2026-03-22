using Polly.Retry;
using Polly.CircuitBreaker;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.Resilience;

/// <summary>
/// Wraps the Polly retry and circuit breaker policies used by the messaging infrastructure.
/// Registered as a singleton to avoid bare policy types in the DI container.
/// </summary>
public class MessagingResiliencePolicies
{
	#region Properties

	/// <summary>
	/// Gets the Polly async retry policy for transient failure recovery.
	/// </summary>
	public AsyncRetryPolicy RetryPolicy { get; }

	/// <summary>
	/// Gets the Polly async circuit breaker policy to prevent cascading failures.
	/// </summary>
	public AsyncCircuitBreakerPolicy CircuitBreakerPolicy { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="MessagingResiliencePolicies"/> with the provided Polly policies.
	/// </summary>
	/// <param name="retryPolicy">The async retry policy. Must not be null.</param>
	/// <param name="circuitBreakerPolicy">The async circuit breaker policy. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when either policy is null.</exception>
	public MessagingResiliencePolicies(AsyncRetryPolicy retryPolicy, AsyncCircuitBreakerPolicy circuitBreakerPolicy)
	{
		RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		CircuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
	}

	#endregion
}
