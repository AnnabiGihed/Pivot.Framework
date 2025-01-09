namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageReceiver;

public interface IMessageReceiver : IDisposable
{
	Task InitializeAsync();
	Task StartListeningAsync();
}
