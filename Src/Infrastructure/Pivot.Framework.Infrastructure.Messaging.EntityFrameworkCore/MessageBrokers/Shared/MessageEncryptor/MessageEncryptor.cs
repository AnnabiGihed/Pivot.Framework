using System.Text;
using System.Security.Cryptography;
using Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;

namespace Pivot.Framework.Infrastructure.Messaging.EntityFrameworkCore.MessageBrokers.Shared.MessageEncryptor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : AES-256 implementation of <see cref="IMessageEncryptor"/>.
///              Encrypts and decrypts message payloads using AES with a 32-byte key.
///              The IV is prepended to the ciphertext for self-contained decryption.
/// </summary>
public class AesMessageEncryptor : IMessageEncryptor
{
	#region Fields

	/// <summary>The 32-byte AES-256 encryption key.</summary>
	protected readonly byte[] _encryptionKey;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new <see cref="AesMessageEncryptor"/> with the specified encryption key.
	/// </summary>
	/// <param name="encryptionKey">A 32-character ASCII string used as the AES-256 key.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="encryptionKey"/> is null or empty.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="encryptionKey"/> is not 32 bytes.</exception>
	public AesMessageEncryptor(string encryptionKey)
	{
		if (string.IsNullOrEmpty(encryptionKey))
			throw new ArgumentNullException(nameof(encryptionKey));

		_encryptionKey = Encoding.UTF8.GetBytes(encryptionKey);

		if (_encryptionKey.Length != 32)
		{
			throw new ArgumentException("EncryptionKey must be 32 bytes (ASCII characters) long for AES-256 encryption.");
		}
	}
	#endregion

	#region IMessageEncryptor Implementation

	/// <summary>
	/// Encrypts the specified byte array using AES-256-CBC.
	/// The generated IV is prepended to the ciphertext.
	/// </summary>
	/// <param name="data">The raw data to encrypt. Must not be null or empty.</param>
	/// <returns>The encrypted byte array (IV + ciphertext).</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null or empty.</exception>
	public byte[] Encrypt(byte[] data)
	{
		if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));

		using var aes = Aes.Create();
		aes.Key = _encryptionKey;
		aes.GenerateIV();

		using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
		using var memoryStream = new MemoryStream();
		memoryStream.Write(aes.IV, 0, aes.IV.Length);
		using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
		{
			cryptoStream.Write(data, 0, data.Length);
		}
		return memoryStream.ToArray();
	}

	/// <summary>
	/// Decrypts the specified byte array using AES-256-CBC.
	/// Expects the IV to be prepended to the ciphertext (as produced by <see cref="Encrypt"/>).
	/// </summary>
	/// <param name="encryptedData">The encrypted data (IV + ciphertext) to decrypt.</param>
	/// <returns>The decrypted byte array.</returns>
	public byte[] Decrypt(byte[] encryptedData)
	{
		using var aes = Aes.Create();
		aes.Key = _encryptionKey;

		using var memoryStream = new MemoryStream(encryptedData);
		var iv = new byte[aes.BlockSize / 8];
		memoryStream.Read(iv, 0, iv.Length);
		aes.IV = iv;

		using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
		using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
		using var resultStream = new MemoryStream();
		cryptoStream.CopyTo(resultStream);
		return resultStream.ToArray();
	}
	#endregion
}