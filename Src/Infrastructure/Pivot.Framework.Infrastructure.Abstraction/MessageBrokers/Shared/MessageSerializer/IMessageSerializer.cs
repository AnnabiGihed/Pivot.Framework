namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageSerializer;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for serializing and deserializing message payloads
///              for transmission through the message broker.
/// </summary>
public interface IMessageSerializer
{
	#region Methods

	/// <summary>
	/// Serializes the specified message object into a byte array.
	/// </summary>
	/// <typeparam name="T">The type of the message to serialize.</typeparam>
	/// <param name="message">The message object to serialize.</param>
	/// <returns>A byte array representing the serialized message.</returns>
	byte[] Serialize<T>(T message);

	/// <summary>
	/// Deserializes the specified byte array back into a message object.
	/// </summary>
	/// <typeparam name="T">The type of the message to deserialize into.</typeparam>
	/// <param name="data">The byte array to deserialize.</param>
	/// <returns>The deserialized message object.</returns>
	T Deserialize<T>(byte[] data);

	#endregion
}
