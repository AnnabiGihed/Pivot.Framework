namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageSerializer;

public interface IMessageSerializer
{
	byte[] Serialize<T>(T message);
}

