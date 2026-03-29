using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BffResponse{T}"/>.
///              Verifies Ok, Degraded, and Unavailable factory methods,
///              availability status, degraded components, and retry-after behaviour.
/// </summary>
public class BffResponseTests
{
	#region Ok Tests

	[Fact]
	public void Ok_ShouldCreateFullAvailabilityResponse()
	{
		var response = BffResponse<string>.Ok("data");

		response.Data.Should().Be("data");
		response.Availability.Should().Be(DataAvailability.Full);
		response.DegradedComponents.Should().BeEmpty();
		response.RetryAfterSeconds.Should().BeNull();
		response.IsFullyAvailable.Should().BeTrue();
	}

	#endregion

	#region Degraded Tests

	[Fact]
	public void Degraded_ShouldCreateDegradedResponse()
	{
		var component = new DegradedComponent
		{
			ServiceName = "InventoryService",
			AffectedSection = "StockLevels",
			Status = DataAvailability.Unavailable,
			Reason = "Timeout"
		};

		var response = BffResponse<string>.Degraded("partial-data", component);

		response.Data.Should().Be("partial-data");
		response.Availability.Should().Be(DataAvailability.Degraded);
		response.DegradedComponents.Should().HaveCount(1);
		response.DegradedComponents[0].ServiceName.Should().Be("InventoryService");
		response.DegradedComponents[0].AffectedSection.Should().Be("StockLevels");
		response.DegradedComponents[0].Reason.Should().Be("Timeout");
		response.IsFullyAvailable.Should().BeFalse();
	}

	[Fact]
	public void Degraded_WithMultipleComponents_ShouldListAll()
	{
		var c1 = new DegradedComponent { ServiceName = "Svc1", AffectedSection = "A" };
		var c2 = new DegradedComponent { ServiceName = "Svc2", AffectedSection = "B" };

		var response = BffResponse<int>.Degraded(42, c1, c2);

		response.DegradedComponents.Should().HaveCount(2);
	}

	#endregion

	#region Unavailable Tests

	[Fact]
	public void Unavailable_ShouldCreateUnavailableResponseWithRetryAfter()
	{
		var component = new DegradedComponent
		{
			ServiceName = "AuthService",
			AffectedSection = "Login",
			Status = DataAvailability.Unavailable
		};

		var response = BffResponse<string>.Unavailable(30, component);

		response.Data.Should().BeNull();
		response.Availability.Should().Be(DataAvailability.Unavailable);
		response.RetryAfterSeconds.Should().Be(30);
		response.DegradedComponents.Should().HaveCount(1);
		response.IsFullyAvailable.Should().BeFalse();
	}

	#endregion

	#region IsFullyAvailable Tests

	[Fact]
	public void IsFullyAvailable_WhenFullButHasDegradedComponents_ShouldBeFalse()
	{
		var response = new BffResponse<string>
		{
			Data = "data",
			Availability = DataAvailability.Full,
			DegradedComponents = new List<DegradedComponent>
			{
				new() { ServiceName = "Svc", AffectedSection = "Section" }
			}
		};

		response.IsFullyAvailable.Should().BeFalse();
	}

	#endregion
}
