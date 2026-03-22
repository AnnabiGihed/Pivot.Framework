using System.Text;
using System.Text.Json;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageSerializer;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageSerializer;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : System.Text.Json-based implementation of <see cref="IMessageSerializer"/>.
///              Serializes and deserializes message payloads to/from UTF-8 encoded byte arrays.
/// </summary>
public class JsonMessageSerializer : IMessageSerializer
{
	#region IMessageSerializer Implementation

	/// <summary>
	/// Serializes the specified message object into a UTF-8 encoded byte array using System.Text.Json.
	/// </summary>
	/// <typeparam name="T">The type of the message to serialize.</typeparam>
	/// <param name="message">The message object to serialize. Must not be null.</param>
	/// <returns>A byte array representing the serialized message.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
	public byte[] Serialize<T>(T message)
	{
		if (message == null) throw new ArgumentNullException(nameof(message));
		return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
	}

	/// <summary>
	/// Deserializes the specified UTF-8 encoded byte array back into a message object using System.Text.Json.
	/// </summary>
	/// <typeparam name="T">The type of the message to deserialize into.</typeparam>
	/// <param name="data">The byte array to deserialize. Must not be null or empty.</param>
	/// <returns>The deserialized message object.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null or empty.</exception>
	/// <exception cref="InvalidOperationException">Thrown when deserialization returns null.</exception>
	public T Deserialize<T>(byte[] data)
	{
		if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));
		return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data))
			?? throw new InvalidOperationException("Deserialization returned null.");
	}
	#endregion
}
