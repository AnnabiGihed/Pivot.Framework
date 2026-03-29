using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Coordinator;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.EventStore;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ProjectionRegistration"/> and <see cref="ProjectionVersionPromotedEvent"/>.
///              Verifies model properties, state enum values, and event construction.
/// </summary>
public class ProjectionRegistrationTests
{
	#region ProjectionRegistration Tests

	[Fact]
	public void ProjectionRegistration_ShouldSetAllProperties()
	{
		var reg = new ProjectionRegistration
		{
			Id = Guid.NewGuid(),
			ProjectionName = "OrderProjection",
			ActiveVersion = 3,
			RebuildTargetVersion = 4,
			State = ProjectionState.Rebuilding,
			RebuildFinalPosition = 50000,
			ParityCheckPassed = true,
			LastParityCheckUtc = DateTime.UtcNow,
			CreatedAtUtc = DateTime.UtcNow,
			LastUpdatedUtc = DateTime.UtcNow
		};

		reg.ProjectionName.Should().Be("OrderProjection");
		reg.ActiveVersion.Should().Be(3);
		reg.RebuildTargetVersion.Should().Be(4);
		reg.State.Should().Be(ProjectionState.Rebuilding);
		reg.RebuildFinalPosition.Should().Be(50000);
		reg.ParityCheckPassed.Should().BeTrue();
	}

	#endregion

	#region ProjectionState Tests

	[Fact]
	public void ProjectionState_ShouldHaveAllExpectedValues()
	{
		Enum.GetValues<ProjectionState>().Should().HaveCount(6);
		Enum.IsDefined(ProjectionState.Active).Should().BeTrue();
		Enum.IsDefined(ProjectionState.RebuildPlanned).Should().BeTrue();
		Enum.IsDefined(ProjectionState.Rebuilding).Should().BeTrue();
		Enum.IsDefined(ProjectionState.ParityChecking).Should().BeTrue();
		Enum.IsDefined(ProjectionState.PromotionReady).Should().BeTrue();
		Enum.IsDefined(ProjectionState.Deprecated).Should().BeTrue();
	}

	#endregion

	#region ProjectionVersionPromotedEvent Tests

	[Fact]
	public void ProjectionVersionPromotedEvent_ShouldSetProperties()
	{
		var evt = new ProjectionVersionPromotedEvent
		{
			ProjectionName = "RecordProjection",
			OldVersion = 1,
			NewVersion = 2,
			LastEventPosition = 99999
		};

		evt.ProjectionName.Should().Be("RecordProjection");
		evt.OldVersion.Should().Be(1);
		evt.NewVersion.Should().Be(2);
		evt.LastEventPosition.Should().Be(99999);
		evt.Id.Should().NotBe(Guid.Empty);
		evt.OccurredOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
	}

	#endregion
}
