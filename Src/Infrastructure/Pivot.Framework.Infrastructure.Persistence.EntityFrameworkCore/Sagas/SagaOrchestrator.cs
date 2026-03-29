using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Domain.Sagas;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Application.Abstractions.Sagas;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Orchestrator-based saga implementation that executes saga steps sequentially,
///              persists state after each transition, and performs compensation (rollback)
///              on failure by walking backward through completed steps.
///
///              The orchestrator is persistence-context-generic so that saga state is stored
///              in the same database as the bounded context's business data, enabling
///              transactional consistency between saga state and business state.
///
///              Key behaviours:
///              - Steps are executed in order (index 0 → N).
///              - On step failure, the saga transitions to <see cref="SagaState.Compensating"/>
///                and walks backward through completed steps calling <see cref="ISagaStep{TSagaData}.CompensateAsync"/>.
///              - If all compensations succeed, the saga transitions to <see cref="SagaState.Compensated"/>.
///              - If any compensation fails, the saga transitions to <see cref="SagaState.Failed"/>
///                and the failure reason is recorded for manual investigation.
///              - State is persisted after each step transition to support crash recovery.
/// </summary>
/// <typeparam name="TContext">The persistence context type for saga repository resolution.</typeparam>
public sealed class SagaOrchestrator<TContext> : ISagaOrchestrator
	where TContext : class, IPersistenceContext
{
	#region Fields

	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		TypeNameHandling = TypeNameHandling.None,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		Formatting = Formatting.None
	};

	private readonly ISagaRepository<TContext> _sagaRepository;
	private readonly ILogger<SagaOrchestrator<TContext>> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="SagaOrchestrator{TContext}"/>.
	/// </summary>
	/// <param name="sagaRepository">The saga repository for persistence.</param>
	/// <param name="logger">Logger for diagnostic tracing.</param>
	public SagaOrchestrator(
		ISagaRepository<TContext> sagaRepository,
		ILogger<SagaOrchestrator<TContext>> logger)
	{
		_sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region ISagaOrchestrator Implementation

	/// <inheritdoc />
	public async Task<Result<Guid>> StartAsync<TSagaData>(
		ISagaDefinition<TSagaData> definition,
		TSagaData data,
		string? correlationId = null,
		CancellationToken cancellationToken = default)
		where TSagaData : class
	{
		ArgumentNullException.ThrowIfNull(definition);
		ArgumentNullException.ThrowIfNull(data);

		if (definition.Steps.Count == 0)
			return Result.Failure<Guid>(new Error("Saga.NoSteps", $"Saga '{definition.SagaType}' has no steps defined."));

		var instance = new SagaInstance
		{
			Id = Guid.NewGuid(),
			SagaType = definition.SagaType,
			State = SagaState.InProgress,
			CurrentStepIndex = 0,
			SerializedData = JsonConvert.SerializeObject(data, SerializerSettings),
			CorrelationId = correlationId,
			StartedAtUtc = DateTime.UtcNow,
			Version = 1
		};

		await _sagaRepository.AddAsync(instance, cancellationToken);

		// Create step records for all steps
		for (var i = 0; i < definition.Steps.Count; i++)
		{
			var stepRecord = new SagaStepRecord
			{
				Id = Guid.NewGuid(),
				SagaInstanceId = instance.Id,
				StepName = definition.Steps[i].StepName,
				StepIndex = i,
				Status = SagaStepStatus.Pending
			};
			await _sagaRepository.AddStepRecordAsync(stepRecord, cancellationToken);
		}

		await _sagaRepository.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Saga '{SagaType}' ({SagaId}) started with {StepCount} steps. CorrelationId: {CorrelationId}",
			definition.SagaType, instance.Id, definition.Steps.Count, correlationId);

		// Execute steps
		var result = await ExecuteStepsAsync(instance, definition, data, cancellationToken);

		if (result.IsFailure)
			return Result.Failure<Guid>(result.Error);

		return Result.Success(instance.Id);
	}

	/// <inheritdoc />
	public async Task<Result> ResumeAsync<TSagaData>(
		Guid sagaId,
		ISagaDefinition<TSagaData> definition,
		CancellationToken cancellationToken = default)
		where TSagaData : class
	{
		var instance = await _sagaRepository.GetByIdAsync(sagaId, cancellationToken);
		if (instance is null)
			return Result.Failure(new Error("Saga.NotFound", $"Saga instance '{sagaId}' not found."));

		if (instance.State is SagaState.Completed or SagaState.Compensated or SagaState.Failed)
			return Result.Failure(new Error("Saga.AlreadyTerminated", $"Saga '{sagaId}' is in terminal state '{instance.State}'."));

		var data = JsonConvert.DeserializeObject<TSagaData>(instance.SerializedData ?? "{}", SerializerSettings);
		if (data is null)
			return Result.Failure(new Error("Saga.DeserializationFailed", $"Cannot deserialize saga data for '{sagaId}'."));

		_logger.LogInformation(
			"Resuming saga '{SagaType}' ({SagaId}) from step index {StepIndex}, state: {State}",
			instance.SagaType, instance.Id, instance.CurrentStepIndex, instance.State);

		if (instance.State == SagaState.Compensating)
		{
			return await CompensateAsync(instance, definition, data, cancellationToken);
		}

		return await ExecuteStepsAsync(instance, definition, data, cancellationToken);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Executes saga steps sequentially starting from the current step index.
	/// On failure, transitions to compensation.
	/// </summary>
	private async Task<Result> ExecuteStepsAsync<TSagaData>(
		SagaInstance instance,
		ISagaDefinition<TSagaData> definition,
		TSagaData data,
		CancellationToken cancellationToken)
		where TSagaData : class
	{
		var stepRecords = await _sagaRepository.GetStepRecordsAsync(instance.Id, cancellationToken);

		for (var i = instance.CurrentStepIndex; i < definition.Steps.Count; i++)
		{
			var step = definition.Steps[i];
			var record = stepRecords.FirstOrDefault(r => r.StepIndex == i);

			if (record is null)
				continue;

			// Mark step as executing
			record.Status = SagaStepStatus.Executing;
			record.StartedAtUtc = DateTime.UtcNow;
			instance.CurrentStepIndex = i;
			instance.Version++;
			await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);
			await _sagaRepository.UpdateAsync(instance, cancellationToken);
			await _sagaRepository.SaveChangesAsync(cancellationToken);

			_logger.LogDebug(
				"Saga ({SagaId}) executing step {StepIndex}: '{StepName}'",
				instance.Id, i, step.StepName);

			var stepResult = await step.ExecuteAsync(data, cancellationToken);

			if (stepResult.IsFailure)
			{
				record.Status = SagaStepStatus.Failed;
				record.CompletedAtUtc = DateTime.UtcNow;
				record.Error = stepResult.Error?.Message;
				await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);

				_logger.LogWarning(
					"Saga ({SagaId}) step {StepIndex} '{StepName}' failed: {Error}. Starting compensation.",
					instance.Id, i, step.StepName, stepResult.Error);

				// Persist updated saga data before compensation
				instance.SerializedData = JsonConvert.SerializeObject(data, SerializerSettings);
				instance.State = SagaState.Compensating;
				instance.FailureReason = stepResult.Error?.Message;
				instance.Version++;
				await _sagaRepository.UpdateAsync(instance, cancellationToken);
				await _sagaRepository.SaveChangesAsync(cancellationToken);

				return await CompensateAsync(instance, definition, data, cancellationToken);
			}

			// Step succeeded
			record.Status = SagaStepStatus.Completed;
			record.CompletedAtUtc = DateTime.UtcNow;
			instance.SerializedData = JsonConvert.SerializeObject(data, SerializerSettings);
			instance.Version++;
			await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);
			await _sagaRepository.UpdateAsync(instance, cancellationToken);
			await _sagaRepository.SaveChangesAsync(cancellationToken);

			_logger.LogDebug(
				"Saga ({SagaId}) step {StepIndex} '{StepName}' completed successfully.",
				instance.Id, i, step.StepName);
		}

		// All steps completed
		instance.State = SagaState.Completed;
		instance.CompletedAtUtc = DateTime.UtcNow;
		instance.Version++;
		await _sagaRepository.UpdateAsync(instance, cancellationToken);
		await _sagaRepository.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Saga '{SagaType}' ({SagaId}) completed successfully.",
			instance.SagaType, instance.Id);

		return Result.Success();
	}

	/// <summary>
	/// Compensates completed steps in reverse order.
	/// </summary>
	private async Task<Result> CompensateAsync<TSagaData>(
		SagaInstance instance,
		ISagaDefinition<TSagaData> definition,
		TSagaData data,
		CancellationToken cancellationToken)
		where TSagaData : class
	{
		var stepRecords = await _sagaRepository.GetStepRecordsAsync(instance.Id, cancellationToken);

		// Walk backward through completed steps
		for (var i = instance.CurrentStepIndex - 1; i >= 0; i--)
		{
			var step = definition.Steps[i];
			var record = stepRecords.FirstOrDefault(r => r.StepIndex == i);

			if (record is null || record.Status != SagaStepStatus.Completed)
				continue;

			record.Status = SagaStepStatus.Compensating;
			instance.CurrentStepIndex = i;
			instance.Version++;
			await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);
			await _sagaRepository.UpdateAsync(instance, cancellationToken);
			await _sagaRepository.SaveChangesAsync(cancellationToken);

			_logger.LogDebug(
				"Saga ({SagaId}) compensating step {StepIndex}: '{StepName}'",
				instance.Id, i, step.StepName);

			var compensateResult = await step.CompensateAsync(data, cancellationToken);

			if (compensateResult.IsFailure)
			{
				record.Status = SagaStepStatus.Failed;
				record.CompletedAtUtc = DateTime.UtcNow;
				record.Error = $"Compensation failed: {compensateResult.Error?.Message}";

				instance.State = SagaState.Failed;
				instance.CompletedAtUtc = DateTime.UtcNow;
				instance.FailureReason = $"Compensation failed at step '{step.StepName}': {compensateResult.Error?.Message}";
				instance.Version++;

				await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);
				await _sagaRepository.UpdateAsync(instance, cancellationToken);
				await _sagaRepository.SaveChangesAsync(cancellationToken);

				_logger.LogError(
					"Saga ({SagaId}) compensation FAILED at step {StepIndex} '{StepName}': {Error}. Manual intervention required.",
					instance.Id, i, step.StepName, compensateResult.Error);

				return Result.Failure(new Error("Saga.CompensationFailed", instance.FailureReason));
			}

			record.Status = SagaStepStatus.Compensated;
			record.CompletedAtUtc = DateTime.UtcNow;
			await _sagaRepository.UpdateStepRecordAsync(record, cancellationToken);
			await _sagaRepository.SaveChangesAsync(cancellationToken);

			_logger.LogDebug(
				"Saga ({SagaId}) step {StepIndex} '{StepName}' compensated successfully.",
				instance.Id, i, step.StepName);
		}

		// All compensations completed
		instance.State = SagaState.Compensated;
		instance.CompletedAtUtc = DateTime.UtcNow;
		instance.Version++;
		await _sagaRepository.UpdateAsync(instance, cancellationToken);
		await _sagaRepository.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Saga '{SagaType}' ({SagaId}) fully compensated.",
			instance.SagaType, instance.Id);

		return Result.Failure(new Error("Saga.Compensated",
			$"Saga '{instance.SagaType}' was compensated due to: {instance.FailureReason}"));
	}

	#endregion
}
