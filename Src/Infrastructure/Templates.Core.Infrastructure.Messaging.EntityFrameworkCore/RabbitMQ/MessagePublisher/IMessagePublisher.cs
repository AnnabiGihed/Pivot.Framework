namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessagePublisher;

public interface IMessagePublisher
{
	Task PublishAsync<T>(T message);
}