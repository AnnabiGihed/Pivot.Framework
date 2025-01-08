namespace Temlates.Core.Infrastructure.Messaging.EntityFrameworkCore.RabbitMQ.MessageCompressor;

public interface IMessageCompressor
{
	byte[] Compress(byte[] dataToCompress);
	public byte[] Decompress(byte[] compressedData);
}
