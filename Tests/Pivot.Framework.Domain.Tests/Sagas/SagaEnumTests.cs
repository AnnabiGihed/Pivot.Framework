using FluentAssertions;
using Pivot.Framework.Domain.Sagas;

namespace Pivot.Framework.Domain.Tests.Sagas;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SagaState"/> and <see cref="SagaStepStatus"/> enums.
///              Verifies enum values and completeness.
/// </summary>
public class SagaEnumTests
{
	#region SagaState Tests

	/// <summary>
	/// Verifies that SagaState has all expected values.
	/// </summary>
	[Fact]
	public void SagaState_ShouldHaveAllExpectedValues()
	{
		Enum.GetNames<SagaState>().Should().HaveCount(6);
		((int)SagaState.NotStarted).Should().Be(0);
		((int)SagaState.InProgress).Should().Be(1);
		((int)SagaState.Completed).Should().Be(2);
		((int)SagaState.Compensating).Should().Be(3);
		((int)SagaState.Compensated).Should().Be(4);
		((int)SagaState.Failed).Should().Be(5);
	}

	#endregion

	#region SagaStepStatus Tests

	/// <summary>
	/// Verifies that SagaStepStatus has all expected values.
	/// </summary>
	[Fact]
	public void SagaStepStatus_ShouldHaveAllExpectedValues()
	{
		Enum.GetNames<SagaStepStatus>().Should().HaveCount(6);
		((int)SagaStepStatus.Pending).Should().Be(0);
		((int)SagaStepStatus.Executing).Should().Be(1);
		((int)SagaStepStatus.Completed).Should().Be(2);
		((int)SagaStepStatus.Compensating).Should().Be(3);
		((int)SagaStepStatus.Compensated).Should().Be(4);
		((int)SagaStepStatus.Failed).Should().Be(5);
	}

	#endregion
}
