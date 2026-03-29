namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Versioning;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Contract for upgrading events from one schema version to another.
///              Implementations handle the transformation of event payloads during replay
///              when the event schema has evolved since the original event was stored.
///
///              Register one upgrader per event type version transition
///              (e.g., OrderCreatedV1 -> OrderCreatedV2).
/// </summary>
/// <typeparam name="TEvent">The target event type after upgrade.</typeparam>
public interface IEventUpgrader<TEvent>
{
	/// <summary>
	/// Gets the source event version this upgrader handles.
	/// </summary>
	int FromVersion { get; }

	/// <summary>
	/// Gets the target event version after upgrade.
	/// </summary>
	int ToVersion { get; }

	/// <summary>
	/// Upgrades a serialized event payload from <see cref="FromVersion"/> to <see cref="ToVersion"/>.
	/// </summary>
	/// <param name="payload">The serialized event payload (JSON) in the old schema.</param>
	/// <returns>The upgraded event instance.</returns>
	TEvent Upgrade(string payload);
}
