using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Inbox.Repositories;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Inbox.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="IInboxRepository{TContext}"/>.
///              Uses the existing <see cref="OutboxMessageConsumer"/> entity (composite key:
///              MessageId + ConsumerName) to track idempotent message consumption.
///              SaveChanges is NOT called inside repository methods — the caller
///              (or the ambient unit of work) is responsible for committing.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext that contains the inbox table.</typeparam>
public sealed class InboxRepository<TContext>(TContext dbContext) : IInboxRepository<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Fields

	private readonly TContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<OutboxMessageConsumer>()
			.AnyAsync(c => c.Id == messageId && c.Name == consumerName, cancellationToken);
	}

	/// <inheritdoc />
	public Task<Result> RecordConsumptionAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
	{
		var consumer = new OutboxMessageConsumer
		{
			Id = messageId,
			Name = consumerName
		};

		_dbContext.Set<OutboxMessageConsumer>().Add(consumer);
		return Task.FromResult(Result.Success());
	}

	#endregion
}
