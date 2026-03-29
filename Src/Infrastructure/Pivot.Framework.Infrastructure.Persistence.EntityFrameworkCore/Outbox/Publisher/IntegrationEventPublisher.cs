using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Domain.Primitives;
using Pivot.Framework.Application.Abstractions.Correlation;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.IntegrationEventPublisher;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Outbox.Publisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Persists integration events into the Outbox as serialized messages
///              stamped with <see cref="MessageKind.IntegrationEvent"/>.
///              This ensures the outbox processor always routes these messages to the
///              external message broker, never dispatching them in-process only.
///
///              Follows the same transactional pattern as <see cref="DomainEventPublisher{TContext}"/>:
///              the message is added to the change tracker and committed atomically with
///              the business data via the ambient unit of work.
///
///              Automatically captures the ambient <see cref="CorrelationContext.CorrelationId"/>
///              for end-to-end distributed tracing.
/// </summary>
/// <typeparam name="TContext">EF Core DbContext type that stores the outbox table.</typeparam>
public sealed class IntegrationEventPublisher<TContext> : IIntegrationEventPublisher
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
	private readonly ILogger<IntegrationEventPublisher<TContext>> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="IntegrationEventPublisher{TContext}"/>.
	/// </summary>
	/// <param name="outboxRepository">The outbox repository for persisting messages.</param>
	/// <param name="logger">Logger for diagnostic tracing.</param>
	public IntegrationEventPublisher(
		IOutboxRepository<TContext> outboxRepository,
		ILogger<IntegrationEventPublisher<TContext>> logger)
	{
		_outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<Result> PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(integrationEvent);

			var eventType = integrationEvent.GetType().AssemblyQualifiedName;
			if (string.IsNullOrWhiteSpace(eventType))
				return Result.Failure(new Error("IntegrationEventTypeError", "Integration event type name could not be resolved."));

			var payload = JsonConvert.SerializeObject(integrationEvent, SerializerSettings);

			var outboxMessage = new OutboxMessage
			{
				Id = integrationEvent.Id,
				Payload = payload,
				CreatedAtUtc = integrationEvent.OccurredOnUtc,
				EventType = eventType,
				CorrelationId = integrationEvent.CorrelationId ?? CorrelationContext.CorrelationId,
				Kind = MessageKind.IntegrationEvent
			};

			_logger.LogDebug(
				"Enqueueing integration event to outbox: {EventType} ({EventId}), CorrelationId: {CorrelationId}",
				eventType, integrationEvent.Id, outboxMessage.CorrelationId);

			return await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while enqueueing integration event to outbox.");
			return Result.Failure(new Error("IntegrationEventPublishError", $"Error while publishing integration event: {ex.Message}"));
		}
	}

	#endregion
}
