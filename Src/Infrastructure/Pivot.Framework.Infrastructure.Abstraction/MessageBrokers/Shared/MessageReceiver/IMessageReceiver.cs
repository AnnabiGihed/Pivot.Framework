namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessageReceiver;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for receiving messages from the message broker.
///              Implements <see cref="IDisposable"/> to manage broker connection resources.
/// </summary>
public interface IMessageReceiver : IDisposable
{
	#region Methods

	/// <summary>
	/// Initializes the receiver, setting up the connection and channel to the message broker.
	/// </summary>
	Task InitializeAsync();

	/// <summary>
	/// Starts listening for incoming messages from the message broker.
	/// </summary>
	Task StartListeningAsync();

	/// <summary>
	/// Stops listening for incoming messages gracefully.
	/// </summary>
	/// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
	Task StopListeningAsync(CancellationToken ct = default) => Task.CompletedTask;

	#endregion
}
