using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Sagas.Repositories;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : EF Core implementation of <see cref="ISagaRepository{TContext}"/>.
///              Provides persistence operations for saga instances and their step records.
/// </summary>
/// <typeparam name="TContext">The EF Core DbContext that contains the saga tables.</typeparam>
public sealed class SagaRepository<TContext>(TContext dbContext) : ISagaRepository<TContext>
	where TContext : DbContext, IPersistenceContext
{
	#region Fields

	private readonly TContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	#endregion

	#region Saga Instance Methods

	/// <inheritdoc />
	public async Task<SagaInstance?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<SagaInstance>()
			.FindAsync(new object[] { sagaId }, cancellationToken);
	}

	/// <inheritdoc />
	public Task<Result> AddAsync(SagaInstance instance, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<SagaInstance>().Add(instance);
		return Task.FromResult(Result.Success());
	}

	/// <inheritdoc />
	public Task<Result> UpdateAsync(SagaInstance instance, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<SagaInstance>().Update(instance);
		return Task.FromResult(Result.Success());
	}

	#endregion

	#region Step Record Methods

	/// <inheritdoc />
	public async Task<IReadOnlyList<SagaStepRecord>> GetStepRecordsAsync(Guid sagaId, CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<SagaStepRecord>()
			.Where(r => r.SagaInstanceId == sagaId)
			.OrderBy(r => r.StepIndex)
			.ToListAsync(cancellationToken);
	}

	/// <inheritdoc />
	public Task<Result> AddStepRecordAsync(SagaStepRecord record, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<SagaStepRecord>().Add(record);
		return Task.FromResult(Result.Success());
	}

	/// <inheritdoc />
	public Task<Result> UpdateStepRecordAsync(SagaStepRecord record, CancellationToken cancellationToken = default)
	{
		_dbContext.Set<SagaStepRecord>().Update(record);
		return Task.FromResult(Result.Success());
	}

	#endregion

	#region Persistence

	/// <inheritdoc />
	public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	#endregion
}
