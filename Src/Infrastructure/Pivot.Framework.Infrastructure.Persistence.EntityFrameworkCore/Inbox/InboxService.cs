using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Inbox;
using Pivot.Framework.Infrastructure.Abstraction.Inbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Non-generic inbox service implementation that wraps a context-specific
///              <see cref="IInboxRepository{TContext}"/> and exposes <see cref="SaveChangesAsync"/>
///              for persisting inbox records.
///
///              Injected into non-generic consumers (e.g., <c>RabbitMQReceiver</c>,
///              <c>InProcessMessagePublisher</c>) that need to check-and-record message
///              consumption without being generic over <typeparamref name="TContext"/>.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext that contains the inbox table.</typeparam>
public sealed class InboxService<TContext> : IInboxService
	where TContext : DbContext, IPersistenceContext
{
	#region Fields

	private readonly IInboxRepository<TContext> _inboxRepository;
	private readonly TContext _dbContext;
	private readonly ILogger<InboxService<TContext>> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="InboxService{TContext}"/>.
	/// </summary>
	/// <param name="inboxRepository">The inbox repository for deduplication checks.</param>
	/// <param name="dbContext">The DbContext for persisting consumption records.</param>
	/// <param name="logger">Logger for diagnostic tracing.</param>
	public InboxService(
		IInboxRepository<TContext> inboxRepository,
		TContext dbContext,
		ILogger<InboxService<TContext>> logger)
	{
		_inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region IInboxService Implementation

	/// <inheritdoc />
	public async Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
	{
		return await _inboxRepository.HasBeenProcessedAsync(messageId, consumerName, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<Result> RecordConsumptionAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
	{
		var result = await _inboxRepository.RecordConsumptionAsync(messageId, consumerName, cancellationToken);

		if (result.IsFailure)
		{
			_logger.LogWarning("Failed to record consumption for message {MessageId} by consumer {ConsumerName}: {Error}",
				messageId, consumerName, result.Error);
		}

		return result;
	}

	/// <inheritdoc />
	public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	#endregion
}
