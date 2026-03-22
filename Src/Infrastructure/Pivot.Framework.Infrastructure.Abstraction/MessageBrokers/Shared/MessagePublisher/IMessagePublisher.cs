using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Infrastructure.Abstraction.Outbox.Models;

namespace Pivot.Framework.Infrastructure.Abstraction.MessageBrokers.Shared.MessagePublisher;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Abstraction for publishing outbox messages to the message broker.
///              Implements <see cref="IDisposable"/> to manage broker connection resources.
/// </summary>
public interface IMessagePublisher : IDisposable
{
	#region Methods

	/// <summary>
	/// Publishes the specified outbox message to the message broker asynchronously.
	/// </summary>
	/// <param name="message">The outbox message to publish.</param>
	/// <returns>A <see cref="Result"/> indicating success or failure of the publish operation.</returns>
	Task<Result> PublishAsync(OutboxMessage message);

	#endregion
}
