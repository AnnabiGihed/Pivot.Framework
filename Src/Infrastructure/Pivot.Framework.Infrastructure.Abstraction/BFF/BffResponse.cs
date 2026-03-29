namespace Pivot.Framework.Infrastructure.Abstraction.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : BFF response envelope that supports partial-failure semantics.
///              When a BFF aggregates data from multiple downstream services, each section
///              is annotated with <see cref="DataAvailability"/> so the UI can render
///              degraded/unavailable sections appropriately.
///              Implements MDM spec invariant #32.
/// </summary>
public class BffResponse<T>
{
	/// <summary>The response payload.</summary>
	public T? Data { get; init; }

	/// <summary>Overall availability status of the response.</summary>
	public DataAvailability Availability { get; init; } = DataAvailability.Full;

	/// <summary>List of downstream components that are degraded or unavailable.</summary>
	public List<DegradedComponent> DegradedComponents { get; init; } = new();

	/// <summary>Suggested retry delay in seconds when a write depends on an unavailable service.</summary>
	public int? RetryAfterSeconds { get; init; }

	/// <summary>Whether the response is fully available with no degradation.</summary>
	public bool IsFullyAvailable => Availability == DataAvailability.Full && DegradedComponents.Count == 0;

	public static BffResponse<T> Ok(T data) => new() { Data = data, Availability = DataAvailability.Full };

	public static BffResponse<T> Degraded(T data, params DegradedComponent[] components) => new()
	{
		Data = data,
		Availability = DataAvailability.Degraded,
		DegradedComponents = components.ToList()
	};

	public static BffResponse<T> Unavailable(int retryAfterSeconds, params DegradedComponent[] components) => new()
	{
		Availability = DataAvailability.Unavailable,
		RetryAfterSeconds = retryAfterSeconds,
		DegradedComponents = components.ToList()
	};
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Availability status for a BFF response or response section.
/// </summary>
public enum DataAvailability
{
	Full,
	Degraded,
	Unavailable
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Describes a downstream service component that is degraded or unavailable.
/// </summary>
public sealed class DegradedComponent
{
	/// <summary>Name of the downstream service.</summary>
	public required string ServiceName { get; init; }

	/// <summary>Which response section is affected.</summary>
	public required string AffectedSection { get; init; }

	/// <summary>The availability status of this component.</summary>
	public DataAvailability Status { get; init; } = DataAvailability.Unavailable;

	/// <summary>Optional error message for diagnostics.</summary>
	public string? Reason { get; init; }
}
