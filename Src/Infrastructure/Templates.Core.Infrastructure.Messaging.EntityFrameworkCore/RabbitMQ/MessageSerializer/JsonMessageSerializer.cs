using System.Text;
using System.Text.Json;

namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageSerializer;

public class JsonMessageSerializer : IMessageSerializer
{
	public byte[] Serialize<T>(T message)
	{
		if (message == null) throw new ArgumentNullException(nameof(message));
		return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
	}
}
