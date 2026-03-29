using FluentAssertions;
using Pivot.Framework.Domain.Sagas;
using Pivot.Framework.Infrastructure.Abstraction.Sagas.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SagaInstance"/> and <see cref="SagaStepRecord"/>.
///              Verifies default values and property assignment.
/// </summary>
public class SagaModelTests
{
	#region SagaInstance Tests

	/// <summary>
	/// Verifies that SagaInstance defaults are correct.
	/// </summary>
	[Fact]
	public void SagaInstance_Defaults_ShouldBeCorrect()
	{
		var instance = new SagaInstance();

		instance.State.Should().Be(SagaState.NotStarted);
		instance.CurrentStepIndex.Should().Be(0);
		instance.SagaType.Should().BeEmpty();
		instance.SerializedData.Should().BeNull();
		instance.CorrelationId.Should().BeNull();
		instance.CompletedAtUtc.Should().BeNull();
		instance.FailureReason.Should().BeNull();
		instance.Version.Should().Be(0);
	}

	/// <summary>
	/// Verifies that all SagaInstance properties can be set.
	/// </summary>
	[Fact]
	public void SagaInstance_Properties_ShouldBeSettable()
	{
		var id = Guid.NewGuid();
		var now = DateTime.UtcNow;

		var instance = new SagaInstance
		{
			Id = id,
			SagaType = "OrderPayment",
			State = SagaState.InProgress,
			CurrentStepIndex = 2,
			SerializedData = "{\"orderId\":\"abc\"}",
			CorrelationId = "corr-saga-1",
			StartedAtUtc = now,
			CompletedAtUtc = now.AddMinutes(5),
			FailureReason = "Step 3 failed",
			Version = 3
		};

		instance.Id.Should().Be(id);
		instance.SagaType.Should().Be("OrderPayment");
		instance.State.Should().Be(SagaState.InProgress);
		instance.CurrentStepIndex.Should().Be(2);
		instance.SerializedData.Should().Be("{\"orderId\":\"abc\"}");
		instance.CorrelationId.Should().Be("corr-saga-1");
		instance.StartedAtUtc.Should().Be(now);
		instance.CompletedAtUtc.Should().Be(now.AddMinutes(5));
		instance.FailureReason.Should().Be("Step 3 failed");
		instance.Version.Should().Be(3);
	}

	#endregion

	#region SagaStepRecord Tests

	/// <summary>
	/// Verifies that SagaStepRecord defaults are correct.
	/// </summary>
	[Fact]
	public void SagaStepRecord_Defaults_ShouldBeCorrect()
	{
		var record = new SagaStepRecord();

		record.Status.Should().Be(SagaStepStatus.Pending);
		record.StepName.Should().BeEmpty();
		record.StartedAtUtc.Should().BeNull();
		record.CompletedAtUtc.Should().BeNull();
		record.Error.Should().BeNull();
	}

	/// <summary>
	/// Verifies that all SagaStepRecord properties can be set.
	/// </summary>
	[Fact]
	public void SagaStepRecord_Properties_ShouldBeSettable()
	{
		var id = Guid.NewGuid();
		var sagaId = Guid.NewGuid();
		var now = DateTime.UtcNow;

		var record = new SagaStepRecord
		{
			Id = id,
			SagaInstanceId = sagaId,
			StepName = "CreatePaymentIntent",
			StepIndex = 1,
			Status = SagaStepStatus.Completed,
			StartedAtUtc = now,
			CompletedAtUtc = now.AddSeconds(30),
			Error = null
		};

		record.Id.Should().Be(id);
		record.SagaInstanceId.Should().Be(sagaId);
		record.StepName.Should().Be("CreatePaymentIntent");
		record.StepIndex.Should().Be(1);
		record.Status.Should().Be(SagaStepStatus.Completed);
		record.StartedAtUtc.Should().Be(now);
		record.CompletedAtUtc.Should().Be(now.AddSeconds(30));
		record.Error.Should().BeNull();
	}

	#endregion
}
