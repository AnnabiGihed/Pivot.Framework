using Pivot.Framework.Domain.Primitives;

namespace Pivot.Framework.Domain.IntegrationEvents;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Base type for integration events.
///              An IntegrationEvent represents a cross-service boundary occurrence that
///              is published to external consumers via the message broker.
///              It carries a unique identifier, UTC timestamp, and correlation ID for
///              end-to-end distributed tracing.
///
///              Follows the same record-based pattern as <see cref="DomainEvents.DomainEvent"/>
///              but includes <see cref="CorrelationId"/> and is routed differently by the
///              outbox processor (always to the external broker).
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
	#region Constructors

	/// <summary>
	/// Initializes a new integration event with explicit values.
	/// </summary>
	/// <param name="id">Unique identifier of the integration event instance.</param>
	/// <param name="occurredOnUtc">UTC timestamp indicating when the event occurred.</param>
	/// <param name="correlationId">Correlation identifier for distributed tracing. May be null.</param>
	protected IntegrationEvent(Guid id, DateTime occurredOnUtc, string? correlationId)
	{
		if (id == Guid.Empty)
			throw new ArgumentException("Integration event identifier cannot be Guid.Empty.", nameof(id));
		if (occurredOnUtc.Kind != DateTimeKind.Utc)
			throw new ArgumentException("OccurredOnUtc must be expressed in UTC (DateTimeKind.Utc).", nameof(occurredOnUtc));

		Id = id;
		OccurredOnUtc = occurredOnUtc;
		CorrelationId = correlationId;
	}

	/// <summary>
	/// Initializes a new integration event with a generated identifier, current UTC timestamp,
	/// and the ambient correlation ID from <see cref="Application.Abstractions.Correlation.CorrelationContext"/>.
	/// </summary>
	protected IntegrationEvent()
		: this(Guid.NewGuid(), DateTime.UtcNow, null)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets the unique identifier of the integration event instance.
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// Gets the UTC timestamp indicating when the integration event occurred.
	/// </summary>
	public DateTime OccurredOnUtc { get; }

	/// <summary>
	/// Gets the correlation identifier for distributed tracing across services.
	/// </summary>
	public string? CorrelationId { get; init; }

	#endregion
}
