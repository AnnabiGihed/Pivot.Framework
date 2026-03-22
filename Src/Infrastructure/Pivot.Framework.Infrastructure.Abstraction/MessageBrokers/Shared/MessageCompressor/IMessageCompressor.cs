namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageCompressor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for compressing and decompressing message payloads
///              before they are transmitted through the message broker.
/// </summary>
public interface IMessageCompressor
{
	#region Methods

	/// <summary>
	/// Compresses the specified byte array.
	/// </summary>
	/// <param name="dataToCompress">The raw data to compress.</param>
	/// <returns>The compressed byte array.</returns>
	byte[] Compress(byte[] dataToCompress);

	/// <summary>
	/// Decompresses the specified byte array back to its original form.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <returns>The decompressed byte array.</returns>
	byte[] Decompress(byte[] compressedData);

	#endregion
}
