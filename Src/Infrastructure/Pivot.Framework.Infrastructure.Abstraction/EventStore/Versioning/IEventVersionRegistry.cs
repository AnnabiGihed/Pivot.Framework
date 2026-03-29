namespace Pivot.Framework.Infrastructure.Abstraction.EventStore.Versioning;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Registry for event version metadata and upgrader resolution.
///              Used during event replay to determine the current version of an event type
///              and resolve the appropriate chain of upgraders to migrate old events.
/// </summary>
public interface IEventVersionRegistry
{
	/// <summary>
	/// Gets the current (latest) version number for a given event type name.
	/// Returns 1 if no version is registered (default for unversioned events).
	/// </summary>
	int GetCurrentVersion(string eventTypeName);

	/// <summary>
	/// Resolves the deserialized and upgraded event from a stored payload.
	/// Applies the necessary chain of upgraders if the stored version differs
	/// from the current version.
	/// </summary>
	/// <param name="eventTypeName">The stored event type name.</param>
	/// <param name="storedVersion">The version at which the event was stored.</param>
	/// <param name="payload">The serialized event payload.</param>
	/// <returns>The deserialized (and potentially upgraded) event object.</returns>
	object DeserializeAndUpgrade(string eventTypeName, int storedVersion, string payload);
}
