using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Correlation;
using Pivot.Framework.Application.Abstractions.Causation;
using Pivot.Framework.Application.Abstractions.Replay;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.DomainEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Models;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Modified    : 03-2026 — Added aggregate-aware overload that builds a full EventEnvelope
///              with CausationId, ProducerService, AggregateType/Id/Version, and ReplayFlag.
///              Also persists to the event history (event store) in the same transaction.
/// Purpose     : Persists domain events into the Outbox as serialized messages and optionally
///              into the event history table for event sourcing and audit trails.
///              This component does NOT publish to the broker directly.
///              Publishing is handled by the outbox processor after transaction commit.
/// </summary>
/// <typeparam name="TContext">EF Core DbContext type that stores the outbox table.</typeparam>
public sealed class DomainEventPublisher<TContext> : IDomainEventPublisher
	where TContext : DbContext, IPersistenceContext
{
	#region Fields
	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		Formatting = Formatting.None,
		StringEscapeHandling = StringEscapeHandling.Default
	};

	private readonly IOutboxRepository<TContext> _outboxRepository;
	private readonly IEventStoreRepository<TContext>? _eventStoreRepository;
	private readonly ILogger<DomainEventPublisher<TContext>> _logger;
	private readonly string _producerService;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="DomainEventPublisher{TContext}"/> with outbox and optional event store support.
	/// </summary>
	public DomainEventPublisher(
		IOutboxRepository<TContext> outboxRepository,
		ILogger<DomainEventPublisher<TContext>> logger,
		IEventStoreRepository<TContext>? eventStoreRepository = null,
		string? producerService = null)
	{
		_outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_eventStoreRepository = eventStoreRepository;
		_producerService = producerService ?? AppDomain.CurrentDomain.FriendlyName;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Serializes a domain event and stores it in the outbox within the current transaction.
	/// This overload does not capture aggregate metadata in the event envelope.
	/// </summary>
	public async Task<Result> PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(domainEvent);

			var eventType = domainEvent.GetType().AssemblyQualifiedName;
			if (string.IsNullOrWhiteSpace(eventType))
				return Result.Failure(new Error("DomainEventTypeError", "Domain event type name could not be resolved."));

			var payload = JsonConvert.SerializeObject(domainEvent, SerializerSettings);

			var envelope = new EventEnvelope
			{
				EventId = domainEvent.Id,
				EventType = eventType,
				EventVersion = 1,
				OccurredOnUtc = domainEvent.OccurredOnUtc,
				ProducerService = _producerService,
				CorrelationId = CorrelationContext.CorrelationId,
				CausationId = CausationContext.CausationId,
				ReplayFlag = ReplayContext.IsReplaying,
				Payload = payload
			};

			return await PersistEnvelopeAsync(envelope, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while enqueueing domain event to outbox.");
			return Result.Failure(new Error("DomainEventPublishError", $"Error while publishing domain event: {ex.Message}"));
		}
	}

	/// <summary>
	/// Serializes a domain event with full aggregate metadata and stores it in both
	/// the outbox and event history within the current transaction.
	/// </summary>
	public async Task<Result> PublishAsync(IDomainEvent domainEvent, IAggregateRoot aggregate, CancellationToken cancellationToken)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(domainEvent);
			ArgumentNullException.ThrowIfNull(aggregate);

			var eventType = domainEvent.GetType().AssemblyQualifiedName;
			if (string.IsNullOrWhiteSpace(eventType))
				return Result.Failure(new Error("DomainEventTypeError", "Domain event type name could not be resolved."));

			var payload = JsonConvert.SerializeObject(domainEvent, SerializerSettings);

			// Extract aggregate identity — use reflection-safe ToString on the Id property
			var aggregateType = aggregate.GetType().Name;
			var aggregateId = ExtractAggregateId(aggregate);

			var envelope = new EventEnvelope
			{
				EventId = domainEvent.Id,
				EventType = eventType,
				EventVersion = 1,
				OccurredOnUtc = domainEvent.OccurredOnUtc,
				ProducerService = _producerService,
				CorrelationId = CorrelationContext.CorrelationId,
				CausationId = CausationContext.CausationId,
				AggregateType = aggregateType,
				AggregateId = aggregateId,
				AggregateVersion = aggregate.Version,
				ReplayFlag = ReplayContext.IsReplaying,
				Payload = payload
			};

			return await PersistEnvelopeAsync(envelope, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while enqueueing domain event to outbox.");
			return Result.Failure(new Error("DomainEventPublishError", $"Error while publishing domain event: {ex.Message}"));
		}
	}
	#endregion

	#region Private Methods
	private async Task<Result> PersistEnvelopeAsync(EventEnvelope envelope, CancellationToken cancellationToken)
	{
		// Persist to outbox
		var outboxMessage = new OutboxMessage
		{
			Id = envelope.EventId,
			Payload = envelope.Payload,
			CreatedAtUtc = envelope.OccurredOnUtc,
			EventType = envelope.EventType,
			CorrelationId = envelope.CorrelationId,
			Kind = MessageKind.DomainEvent
		};

		_logger.LogDebug("Enqueueing domain event to outbox: {EventType} ({EventId})", envelope.EventType, envelope.EventId);

		var outboxResult = await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
		if (outboxResult.IsFailure)
			return outboxResult;

		// Persist to event history (if event store is configured)
		if (_eventStoreRepository is not null)
		{
			var historyEntry = new EventHistoryEntry
			{
				Id = envelope.EventId,
				EventType = envelope.EventType,
				EventVersion = envelope.EventVersion,
				OccurredOnUtc = envelope.OccurredOnUtc,
				ProducerService = envelope.ProducerService,
				CorrelationId = envelope.CorrelationId,
				CausationId = envelope.CausationId,
				AggregateType = envelope.AggregateType,
				AggregateId = envelope.AggregateId,
				AggregateVersion = envelope.AggregateVersion,
				ReplayFlag = envelope.ReplayFlag,
				Payload = envelope.Payload,
				CreatedAtUtc = DateTime.UtcNow
			};

			var historyResult = await _eventStoreRepository.AppendAsync(historyEntry, cancellationToken);
			if (historyResult.IsFailure)
			{
				_logger.LogWarning("Failed to persist event to event history: {Error}", historyResult.Error);
				return historyResult;
			}
		}

		return Result.Success();
	}

	private static string? ExtractAggregateId(IAggregateRoot aggregate)
	{
		// Use reflection to access the Id property from Entity<TId>
		var idProperty = aggregate.GetType().GetProperty("Id");
		if (idProperty is null) return null;

		var idValue = idProperty.GetValue(aggregate);
		return idValue?.ToString();
	}
	#endregion
}
