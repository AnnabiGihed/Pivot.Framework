namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.RabbitMQ.Models;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Configuration settings for connecting to a RabbitMQ message broker.
///              Holds connection, authentication, and routing details required by the RabbitMQ client.
/// </summary>
public class RabbitMQSettings
{
	#region Properties

	/// <summary>
	/// The port number used to connect to the RabbitMQ server.
	/// </summary>
	public required int Port { get; set; }

	/// <summary>
	/// The name of the queue to consume from or publish to.
	/// </summary>
	public required string Queue { get; set; }

	/// <summary>
	/// The hostname or IP address of the RabbitMQ server.
	/// </summary>
	public required string HostName { get; set; }

	/// <summary>
	/// The username for authenticating with the RabbitMQ server.
	/// </summary>
	public required string UserName { get; set; }

	/// <summary>
	/// The password for authenticating with the RabbitMQ server.
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// The name of the exchange to bind or publish to.
	/// </summary>
	public required string Exchange { get; set; }

	/// <summary>
	/// The routing key used when publishing messages to the exchange.
	/// </summary>
	public required string RoutingKey { get; set; }

	/// <summary>
	/// The virtual host on the RabbitMQ server.
	/// </summary>
	public required string VirtualHost { get; set; }

	/// <summary>
	/// The encryption key used for message-level encryption.
	/// </summary>
	public required string EncryptionKey { get; set; }

	/// <summary>
	/// A client-provided connection name for identification in the RabbitMQ management UI.
	/// </summary>
	public required string ClientProvidedName { get; set; }

	#endregion
}
