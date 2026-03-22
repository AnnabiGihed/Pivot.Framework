using System.IO.Compression;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageCompressor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : GZip-based implementation of <see cref="IMessageCompressor"/>.
///              Compresses message payloads for efficient transport and enforces
///              a maximum decompressed size limit to prevent decompression bombs.
/// </summary>
public class GZipMessageCompressor : IMessageCompressor
{
	#region Constants

	/// <summary>Maximum allowed decompressed payload size (10 MB).</summary>
	private const int MaxDecompressedSize = 10 * 1024 * 1024;

	#endregion

	#region IMessageCompressor Implementation

	/// <summary>
	/// Compresses the specified byte array using GZip with optimal compression level.
	/// </summary>
	/// <param name="dataToCompress">The raw data to compress. Must not be null or empty.</param>
	/// <returns>The GZip-compressed byte array.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="dataToCompress"/> is null or empty.</exception>
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

	/// <summary>
	/// Decompresses the specified GZip-compressed byte array.
	/// Enforces a maximum decompressed size of 10 MB to prevent decompression bomb attacks.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <returns>The decompressed byte array.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the decompressed data exceeds the maximum allowed size.</exception>
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