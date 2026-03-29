using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Pivot.Framework.Containers.API.RealTime;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Base SignalR hub for pushing real-time event updates to connected Blazor circuits.
///              Used for steward queue updates, SLA countdowns, projection lag, and operations dashboard.
///              In Blazor Server, the circuit itself is the real-time channel — this hub provides
///              the server-side push mechanism.
/// </summary>
public class EventNotificationHub : Hub
{
	/// <summary>
	/// Adds the connection to a named group for targeted notifications.
	/// </summary>
	public async Task JoinGroup(string groupName)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
	}

	/// <summary>
	/// Removes the connection from a named group.
	/// </summary>
	public async Task LeaveGroup(string groupName)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Service for pushing real-time event notifications to connected clients/circuits.
///              Used by background services that consume RabbitMQ events to push updates
///              to active Blazor Server circuits via SignalR.
/// </summary>
public sealed class EventPushService
{
	private readonly IHubContext<EventNotificationHub> _hubContext;
	private readonly ILogger<EventPushService> _logger;

	public EventPushService(
		IHubContext<EventNotificationHub> hubContext,
		ILogger<EventPushService> logger)
	{
		_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>Pushes a notification to all connected clients.</summary>
	public async Task NotifyAllAsync(string method, object payload, CancellationToken ct = default)
	{
		await _hubContext.Clients.All.SendAsync(method, payload, ct);
		_logger.LogDebug("Pushed notification '{Method}' to all clients", method);
	}

	/// <summary>Pushes a notification to clients in a specific group.</summary>
	public async Task NotifyGroupAsync(string group, string method, object payload, CancellationToken ct = default)
	{
		await _hubContext.Clients.Group(group).SendAsync(method, payload, ct);
		_logger.LogDebug("Pushed notification '{Method}' to group '{Group}'", method, group);
	}

	/// <summary>Pushes a notification to a specific user.</summary>
	public async Task NotifyUserAsync(string userId, string method, object payload, CancellationToken ct = default)
	{
		await _hubContext.Clients.User(userId).SendAsync(method, payload, ct);
		_logger.LogDebug("Pushed notification '{Method}' to user '{UserId}'", method, userId);
	}
}
