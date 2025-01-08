namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ;

public class RabbitMQSettings
{
	public string HostName { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
	public string VirtualHost { get; set; }
	public int Port { get; set; }
	public string ClientProvidedName { get; set; }
	public string Exchange { get; set; }
	public string Queue { get; set; }
	public string RoutingKey { get; set; }
	public string EncryptionKey { get; set; }
}