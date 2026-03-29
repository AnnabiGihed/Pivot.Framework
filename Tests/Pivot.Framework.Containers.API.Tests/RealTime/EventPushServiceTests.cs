using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pivot.Framework.Containers.API.RealTime;

namespace Pivot.Framework.Containers.API.Tests.RealTime;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="EventPushService"/>.
///              Verifies notification delivery to all clients, groups, and individual users
///              via the SignalR hub context.
/// </summary>
public class EventPushServiceTests
{
	private readonly IHubContext<EventNotificationHub> _hubContext;
	private readonly IHubClients _hubClients;
	private readonly IClientProxy _allProxy;
	private readonly IClientProxy _groupProxy;
	private readonly IClientProxy _userProxy;
	private readonly EventPushService _sut;

	public EventPushServiceTests()
	{
		_hubContext = Substitute.For<IHubContext<EventNotificationHub>>();
		_hubClients = Substitute.For<IHubClients>();
		_allProxy = Substitute.For<IClientProxy>();
		_groupProxy = Substitute.For<IClientProxy>();
		_userProxy = Substitute.For<IClientProxy>();
		var logger = Substitute.For<ILogger<EventPushService>>();

		_hubContext.Clients.Returns(_hubClients);
		_hubClients.All.Returns(_allProxy);

		_sut = new EventPushService(_hubContext, logger);
	}

	#region Constructor Tests

	[Fact]
	public void Constructor_WithNullHubContext_ShouldThrow()
	{
		var act = () => new EventPushService(null!, Substitute.For<ILogger<EventPushService>>());

		act.Should().Throw<ArgumentNullException>().WithParameterName("hubContext");
	}

	[Fact]
	public void Constructor_WithNullLogger_ShouldThrow()
	{
		var act = () => new EventPushService(_hubContext, null!);

		act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
	}

	#endregion

	#region NotifyAllAsync Tests

	[Fact]
	public async Task NotifyAllAsync_ShouldSendToAllClients()
	{
		var payload = new { Message = "test" };

		await _sut.NotifyAllAsync("EventReceived", payload);

		await _allProxy.Received(1).SendCoreAsync(
			"EventReceived",
			Arg.Is<object?[]>(args => args.Length == 1 && args[0] == (object)payload),
			Arg.Any<CancellationToken>());
	}

	#endregion

	#region NotifyGroupAsync Tests

	[Fact]
	public async Task NotifyGroupAsync_ShouldSendToGroup()
	{
		_hubClients.Group("steward-queue").Returns(_groupProxy);
		var payload = new { Count = 5 };

		await _sut.NotifyGroupAsync("steward-queue", "QueueUpdated", payload);

		await _groupProxy.Received(1).SendCoreAsync(
			"QueueUpdated",
			Arg.Is<object?[]>(args => args.Length == 1 && args[0] == (object)payload),
			Arg.Any<CancellationToken>());
	}

	#endregion

	#region NotifyUserAsync Tests

	[Fact]
	public async Task NotifyUserAsync_ShouldSendToUser()
	{
		_hubClients.User("user-42").Returns(_userProxy);
		var payload = new { TaskId = "task-1" };

		await _sut.NotifyUserAsync("user-42", "TaskAssigned", payload);

		await _userProxy.Received(1).SendCoreAsync(
			"TaskAssigned",
			Arg.Is<object?[]>(args => args.Length == 1 && args[0] == (object)payload),
			Arg.Any<CancellationToken>());
	}

	#endregion
}
