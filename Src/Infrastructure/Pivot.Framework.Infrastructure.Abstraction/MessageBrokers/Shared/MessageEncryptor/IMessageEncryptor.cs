namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageEncryptor;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for encrypting and decrypting message payloads
///              to ensure secure transmission through the message broker.
/// </summary>
public interface IMessageEncryptor
{
	#region Methods

	/// <summary>
	/// Encrypts the specified byte array.
	/// </summary>
	/// <param name="data">The raw data to encrypt.</param>
	/// <returns>The encrypted byte array.</returns>
	byte[] Encrypt(byte[] data);

	/// <summary>
	/// Decrypts the specified byte array back to its original form.
	/// </summary>
	/// <param name="encryptedData">The encrypted data to decrypt.</param>
	/// <returns>The decrypted byte array.</returns>
	byte[] Decrypt(byte[] encryptedData);

	#endregion
}
