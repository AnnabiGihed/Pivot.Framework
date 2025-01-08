namespace Templates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageEncryptor;

public interface IMessageEncryptor
{
	byte[] Encrypt(byte[] data);
	byte[] Decrypt(byte[] encryptedData);
}