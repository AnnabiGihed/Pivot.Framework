using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Containers.API.RealTime;
using Pivot.Framework.Containers.API.RealTime.Extensions;

namespace Pivot.Framework.Containers.API.Tests.RealTime;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="SignalRExtensions"/>.
///              Verifies that SignalR services and EventPushService are registered in DI.
/// </summary>
public class SignalRExtensionsTests
{
	[Fact]
	public void AddPivotSignalR_ShouldRegisterEventPushService()
	{
		var services = new ServiceCollection();

		services.AddPivotSignalR();

		services.Should().Contain(sd => sd.ServiceType == typeof(EventPushService));
	}

	[Fact]
	public void AddPivotSignalR_WithNullServices_ShouldThrow()
	{
		IServiceCollection services = null!;

		var act = () => services.AddPivotSignalR();

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void AddPivotSignalR_ShouldReturnSameCollection()
	{
		var services = new ServiceCollection();

		var result = services.AddPivotSignalR();

		result.Should().BeSameAs(services);
	}

	[Fact]
	public void AddPivotSignalR_CalledTwice_ShouldNotDuplicateEventPushService()
	{
		var services = new ServiceCollection();

		services.AddPivotSignalR();
		services.AddPivotSignalR();

		services.Count(sd => sd.ServiceType == typeof(EventPushService)).Should().Be(1);
	}
}
