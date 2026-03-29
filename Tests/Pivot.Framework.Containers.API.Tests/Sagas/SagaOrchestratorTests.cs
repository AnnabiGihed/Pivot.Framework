using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Application.Abstractions.Sagas;
using Pivot.Framework.Domain.Sagas;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Repositories;
using Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Sagas;

namespace Pivot.Framework.Containers.API.Tests.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SagaOrchestrator{TContext}"/>.
///              Verifies happy path execution, failure with compensation, compensation failure,
///              and resume scenarios using mocked saga repository and steps.
/// </summary>
public class SagaOrchestratorTests
{
	#region Test Infrastructure

	public interface ITestContext : IPersistenceContext { }

	private class TestSagaData
	{
		public string Value { get; set; } = string.Empty;
	}

	private class TestSagaDefinition : ISagaDefinition<TestSagaData>
	{
		public string SagaType => "TestSaga";
		public IReadOnlyList<ISagaStep<TestSagaData>> Steps { get; init; } = [];
	}

	private class TestSagaStep : ISagaStep<TestSagaData>
	{
		public string StepName { get; init; } = "TestStep";
		public Func<TestSagaData, CancellationToken, Task<Result>>? OnExecute { get; init; }
		public Func<TestSagaData, CancellationToken, Task<Result>>? OnCompensate { get; init; }

		public Task<Result> ExecuteAsync(TestSagaData data, CancellationToken cancellationToken)
			=> OnExecute?.Invoke(data, cancellationToken) ?? Task.FromResult(Result.Success());

		public Task<Result> CompensateAsync(TestSagaData data, CancellationToken cancellationToken)
			=> OnCompensate?.Invoke(data, cancellationToken) ?? Task.FromResult(Result.Success());
	}

	private readonly ISagaRepository<ITestContext> _sagaRepository = Substitute.For<ISagaRepository<ITestContext>>();
	private readonly ILogger<SagaOrchestrator<ITestContext>> _logger = Substitute.For<ILogger<SagaOrchestrator<ITestContext>>>();

	private SagaOrchestrator<ITestContext> CreateOrchestrator()
		=> new(_sagaRepository, _logger);

	private void SetupRepositoryForStepRecords(int stepCount)
	{
		// Capture added step records so GetStepRecordsAsync returns them
		var stepRecords = new List<SagaStepRecord>();

		_sagaRepository.AddStepRecordAsync(Arg.Any<SagaStepRecord>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				stepRecords.Add(ci.Arg<SagaStepRecord>());
				return Result.Success();
			});

		_sagaRepository.GetStepRecordsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns(_ => stepRecords.OrderBy(r => r.StepIndex).ToList().AsReadOnly());

		_sagaRepository.AddAsync(Arg.Any<SagaInstance>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());
		_sagaRepository.UpdateAsync(Arg.Any<SagaInstance>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());
		_sagaRepository.UpdateStepRecordAsync(Arg.Any<SagaStepRecord>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());
	}

	#endregion

	#region StartAsync - Happy Path Tests

	/// <summary>
	/// Verifies that a saga with all successful steps completes successfully.
	/// </summary>
	[Fact]
	public async Task StartAsync_AllStepsSucceed_ShouldComplete()
	{
		SetupRepositoryForStepRecords(2);
		var orchestrator = CreateOrchestrator();

		var definition = new TestSagaDefinition
		{
			Steps =
			[
				new TestSagaStep { StepName = "Step1" },
				new TestSagaStep { StepName = "Step2" }
			]
		};

		var result = await orchestrator.StartAsync(definition, new TestSagaData { Value = "test" });

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		// Verify saga was marked as completed
		await _sagaRepository.Received().UpdateAsync(
			Arg.Is<SagaInstance>(s => s.State == SagaState.Completed),
			Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that a saga with no steps returns failure.
	/// </summary>
	[Fact]
	public async Task StartAsync_NoSteps_ShouldReturnFailure()
	{
		var orchestrator = CreateOrchestrator();
		var definition = new TestSagaDefinition { Steps = [] };

		var result = await orchestrator.StartAsync(definition, new TestSagaData());

		result.IsFailure.Should().BeTrue();
		result.Error.Code.Should().Be("Saga.NoSteps");
	}

	/// <summary>
	/// Verifies that correlation ID is persisted on the saga instance.
	/// </summary>
	[Fact]
	public async Task StartAsync_WithCorrelationId_ShouldPersistOnInstance()
	{
		SetupRepositoryForStepRecords(1);
		var orchestrator = CreateOrchestrator();
		SagaInstance? capturedInstance = null;

		_sagaRepository.AddAsync(Arg.Any<SagaInstance>(), Arg.Any<CancellationToken>())
			.Returns(ci =>
			{
				capturedInstance = ci.Arg<SagaInstance>();
				return Result.Success();
			});

		var definition = new TestSagaDefinition
		{
			Steps = [new TestSagaStep { StepName = "Step1" }]
		};

		await orchestrator.StartAsync(definition, new TestSagaData(), "corr-123");

		capturedInstance.Should().NotBeNull();
		capturedInstance!.CorrelationId.Should().Be("corr-123");
	}

	#endregion

	#region StartAsync - Failure and Compensation Tests

	/// <summary>
	/// Verifies that when a step fails, previously completed steps are compensated in reverse order.
	/// </summary>
	[Fact]
	public async Task StartAsync_StepFails_ShouldCompensateCompletedSteps()
	{
		SetupRepositoryForStepRecords(3);
		var orchestrator = CreateOrchestrator();
		var compensatedSteps = new List<string>();

		var definition = new TestSagaDefinition
		{
			Steps =
			[
				new TestSagaStep
				{
					StepName = "Step1",
					OnCompensate = (_, _) => { compensatedSteps.Add("Step1"); return Task.FromResult(Result.Success()); }
				},
				new TestSagaStep
				{
					StepName = "Step2",
					OnCompensate = (_, _) => { compensatedSteps.Add("Step2"); return Task.FromResult(Result.Success()); }
				},
				new TestSagaStep
				{
					StepName = "Step3",
					OnExecute = (_, _) => Task.FromResult(Result.Failure(new Error("Step3.Failed", "Step 3 failed")))
				}
			]
		};

		var result = await orchestrator.StartAsync(definition, new TestSagaData());

		result.IsFailure.Should().BeTrue();

		// Steps 1 and 2 should be compensated in reverse order
		compensatedSteps.Should().ContainInOrder("Step2", "Step1");

		// Saga should end as Compensated
		await _sagaRepository.Received().UpdateAsync(
			Arg.Is<SagaInstance>(s => s.State == SagaState.Compensated),
			Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that when compensation itself fails, the saga transitions to Failed state.
	/// </summary>
	[Fact]
	public async Task StartAsync_CompensationFails_ShouldTransitionToFailed()
	{
		SetupRepositoryForStepRecords(2);
		var orchestrator = CreateOrchestrator();

		var definition = new TestSagaDefinition
		{
			Steps =
			[
				new TestSagaStep
				{
					StepName = "Step1",
					OnCompensate = (_, _) => Task.FromResult(Result.Failure(new Error("Comp.Failed", "Compensation failed")))
				},
				new TestSagaStep
				{
					StepName = "Step2",
					OnExecute = (_, _) => Task.FromResult(Result.Failure(new Error("Step2.Failed", "Step 2 failed")))
				}
			]
		};

		var result = await orchestrator.StartAsync(definition, new TestSagaData());

		result.IsFailure.Should().BeTrue();

		// Saga should end as Failed (compensation failure)
		await _sagaRepository.Received().UpdateAsync(
			Arg.Is<SagaInstance>(s => s.State == SagaState.Failed),
			Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Verifies that the first step failing triggers no compensation (nothing to compensate).
	/// </summary>
	[Fact]
	public async Task StartAsync_FirstStepFails_ShouldCompensateToEmptyAndMarkCompensated()
	{
		SetupRepositoryForStepRecords(2);
		var orchestrator = CreateOrchestrator();

		var definition = new TestSagaDefinition
		{
			Steps =
			[
				new TestSagaStep
				{
					StepName = "Step1",
					OnExecute = (_, _) => Task.FromResult(Result.Failure(new Error("Step1.Failed", "Immediate failure")))
				},
				new TestSagaStep { StepName = "Step2" }
			]
		};

		var result = await orchestrator.StartAsync(definition, new TestSagaData());

		result.IsFailure.Should().BeTrue();

		// Should still reach Compensated state (no steps to compensate = vacuously compensated)
		await _sagaRepository.Received().UpdateAsync(
			Arg.Is<SagaInstance>(s => s.State == SagaState.Compensated),
			Arg.Any<CancellationToken>());
	}

	#endregion

	#region ResumeAsync Tests

	/// <summary>
	/// Verifies that resuming a non-existent saga returns failure.
	/// </summary>
	[Fact]
	public async Task ResumeAsync_SagaNotFound_ShouldReturnFailure()
	{
		var orchestrator = CreateOrchestrator();
		_sagaRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((SagaInstance?)null);

		var result = await orchestrator.ResumeAsync(
			Guid.NewGuid(),
			new TestSagaDefinition { Steps = [new TestSagaStep { StepName = "Step1" }] });

		result.IsFailure.Should().BeTrue();
		result.Error.Code.Should().Be("Saga.NotFound");
	}

	/// <summary>
	/// Verifies that resuming a completed saga returns failure.
	/// </summary>
	[Theory]
	[InlineData(SagaState.Completed)]
	[InlineData(SagaState.Compensated)]
	[InlineData(SagaState.Failed)]
	public async Task ResumeAsync_TerminalState_ShouldReturnFailure(SagaState terminalState)
	{
		var orchestrator = CreateOrchestrator();
		var instance = new SagaInstance
		{
			Id = Guid.NewGuid(),
			State = terminalState,
			SagaType = "TestSaga"
		};

		_sagaRepository.GetByIdAsync(instance.Id, Arg.Any<CancellationToken>())
			.Returns(instance);

		var result = await orchestrator.ResumeAsync(
			instance.Id,
			new TestSagaDefinition { Steps = [new TestSagaStep { StepName = "Step1" }] });

		result.IsFailure.Should().BeTrue();
		result.Error.Code.Should().Be("Saga.AlreadyTerminated");
	}

	/// <summary>
	/// Verifies that resuming an InProgress saga continues execution from the current step.
	/// </summary>
	[Fact]
	public async Task ResumeAsync_InProgress_ShouldContinueExecution()
	{
		var orchestrator = CreateOrchestrator();
		var sagaId = Guid.NewGuid();
		var executedSteps = new List<string>();

		var instance = new SagaInstance
		{
			Id = sagaId,
			State = SagaState.InProgress,
			SagaType = "TestSaga",
			CurrentStepIndex = 1,
			SerializedData = "{\"Value\":\"resumed\"}"
		};

		_sagaRepository.GetByIdAsync(sagaId, Arg.Any<CancellationToken>())
			.Returns(instance);
		_sagaRepository.UpdateAsync(Arg.Any<SagaInstance>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());
		_sagaRepository.UpdateStepRecordAsync(Arg.Any<SagaStepRecord>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Step records for steps 0 and 1
		var stepRecords = new List<SagaStepRecord>
		{
			new() { Id = Guid.NewGuid(), SagaInstanceId = sagaId, StepName = "Step1", StepIndex = 0, Status = SagaStepStatus.Completed },
			new() { Id = Guid.NewGuid(), SagaInstanceId = sagaId, StepName = "Step2", StepIndex = 1, Status = SagaStepStatus.Pending }
		};

		_sagaRepository.GetStepRecordsAsync(sagaId, Arg.Any<CancellationToken>())
			.Returns(stepRecords.AsReadOnly());

		var definition = new TestSagaDefinition
		{
			Steps =
			[
				new TestSagaStep
				{
					StepName = "Step1",
					OnExecute = (_, _) => { executedSteps.Add("Step1"); return Task.FromResult(Result.Success()); }
				},
				new TestSagaStep
				{
					StepName = "Step2",
					OnExecute = (_, _) => { executedSteps.Add("Step2"); return Task.FromResult(Result.Success()); }
				}
			]
		};

		var result = await orchestrator.ResumeAsync(sagaId, definition);

		result.IsSuccess.Should().BeTrue();
		// Step2 should have been executed (resumed from index 1)
		executedSteps.Should().Contain("Step2");
		// Step1 should NOT have been re-executed (already completed, resume from index 1)
		executedSteps.Should().NotContain("Step1");
	}

	#endregion

	#region Null Guard Tests

	/// <summary>
	/// Verifies that null definition throws.
	/// </summary>
	[Fact]
	public async Task StartAsync_NullDefinition_ShouldThrow()
	{
		var orchestrator = CreateOrchestrator();

		var act = () => orchestrator.StartAsync<TestSagaData>(null!, new TestSagaData());

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Verifies that null data throws.
	/// </summary>
	[Fact]
	public async Task StartAsync_NullData_ShouldThrow()
	{
		var orchestrator = CreateOrchestrator();
		var definition = new TestSagaDefinition { Steps = [new TestSagaStep { StepName = "Step1" }] };

		var act = () => orchestrator.StartAsync(definition, (TestSagaData)null!);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	#endregion
}
