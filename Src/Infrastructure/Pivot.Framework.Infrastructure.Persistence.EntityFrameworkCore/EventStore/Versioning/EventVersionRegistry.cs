using System.Collections.Concurrent;
using Newtonsoft.Json;
using Pivot.Framework.Infrastructure.Abstraction.EventStore.Versioning;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.EventStore.Versioning;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Default implementation of <see cref="IEventVersionRegistry"/>.
///              Manages event type version metadata and resolves upgrader chains
///              for migrating old event payloads to current schema versions during replay.
/// </summary>
public sealed class EventVersionRegistry : IEventVersionRegistry
{
	private readonly ConcurrentDictionary<string, int> _currentVersions = new();
	private readonly ConcurrentDictionary<string, Type> _eventTypes = new();
	private readonly ConcurrentDictionary<(string EventType, int FromVersion), object> _upgraders = new();

	/// <summary>
	/// Registers an event type with its current version number.
	/// </summary>
	public void RegisterEventType<TEvent>(int currentVersion) where TEvent : class
	{
		var typeName = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName!;
		_currentVersions[typeName] = currentVersion;
		_eventTypes[typeName] = typeof(TEvent);
	}

	/// <summary>
	/// Registers an event type with its current version number using a type name key.
	/// </summary>
	public void RegisterEventType(string eventTypeName, Type eventType, int currentVersion)
	{
		_currentVersions[eventTypeName] = currentVersion;
		_eventTypes[eventTypeName] = eventType;
	}

	/// <summary>
	/// Registers an upgrader for a specific event type and version transition.
	/// </summary>
	public void RegisterUpgrader<TEvent>(IEventUpgrader<TEvent> upgrader)
	{
		var typeName = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName!;
		_upgraders[(typeName, upgrader.FromVersion)] = upgrader;
	}

	/// <inheritdoc />
	public int GetCurrentVersion(string eventTypeName)
	{
		return _currentVersions.GetValueOrDefault(eventTypeName, 1);
	}

	/// <inheritdoc />
	public object DeserializeAndUpgrade(string eventTypeName, int storedVersion, string payload)
	{
		var currentVersion = GetCurrentVersion(eventTypeName);

		// If already at current version, just deserialize normally
		if (storedVersion >= currentVersion)
		{
			if (_eventTypes.TryGetValue(eventTypeName, out var eventType))
			{
				return JsonConvert.DeserializeObject(payload, eventType)
					?? throw new InvalidOperationException($"Failed to deserialize event '{eventTypeName}' v{storedVersion}.");
			}

			var resolvedType = Type.GetType(eventTypeName)
				?? throw new InvalidOperationException($"Cannot resolve event type '{eventTypeName}'.");
			return JsonConvert.DeserializeObject(payload, resolvedType)
				?? throw new InvalidOperationException($"Failed to deserialize event '{eventTypeName}' v{storedVersion}.");
		}

		// Apply upgrader chain: v1 -> v2 -> v3 -> ... -> current
		object result = payload;
		for (var version = storedVersion; version < currentVersion; version++)
		{
			if (!_upgraders.TryGetValue((eventTypeName, version), out var upgrader))
			{
				throw new InvalidOperationException(
					$"No upgrader registered for event '{eventTypeName}' from version {version} to {version + 1}.");
			}

			// Use reflection to call the Upgrade method on the upgrader
			var upgradeMethod = upgrader.GetType().GetMethod("Upgrade")
				?? throw new InvalidOperationException($"Upgrader for '{eventTypeName}' v{version} missing Upgrade method.");

			var inputPayload = result is string s ? s : JsonConvert.SerializeObject(result);
			result = upgradeMethod.Invoke(upgrader, [inputPayload])
				?? throw new InvalidOperationException($"Upgrader for '{eventTypeName}' v{version} returned null.");
		}

		return result;
	}
}
