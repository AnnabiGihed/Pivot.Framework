using System.IO.Compression;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageCompressor;

public class GZipMessageCompressor : IMessageCompressor
{
	private const int MaxDecompressedSize = 10 * 1024 * 1024; // 10 MB

	#region IMessageCompressor Implementation
	public byte[] Compress(byte[] dataToCompress)
	{
		if (dataToCompress == null || dataToCompress.Length == 0) throw new ArgumentNullException(nameof(dataToCompress));

		using var outputStream = new MemoryStream();
		using (var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal))
		{
			compressionStream.Write(dataToCompress, 0, dataToCompress.Length);
		}
		return outputStream.ToArray();
	}
	public byte[] Decompress(byte[] compressedData)
	{
		using var inputStream = new MemoryStream(compressedData);
		using var decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress);
		using var resultStream = new MemoryStream();

		var buffer = new byte[8192];
		int bytesRead;
		int totalBytes = 0;
		while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
		{
			totalBytes += bytesRead;
			if (totalBytes > MaxDecompressedSize)
				throw new InvalidOperationException($"Decompressed data exceeds maximum allowed size of {MaxDecompressedSize} bytes.");
			resultStream.Write(buffer, 0, bytesRead);
		}
		return resultStream.ToArray();
	}
	#endregion
}