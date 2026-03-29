namespace Pivot.Framework.Application.Abstractions.Messaging.Commands;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Marker interface for commands that require idempotent execution.
///              Commands implementing this interface will be automatically deduplicated
///              by the <c>IdempotentCommandBehavior</c> MediatR pipeline behavior
///              using the inbox pattern.
///
///              The <see cref="IdempotencyKey"/> uniquely identifies the intent behind
///              the command. If a command with the same key has already been processed,
///              the pipeline short-circuits and returns a success result without
///              re-executing the handler.
///
///              Usage:
///              <code>
///              public sealed record CreateOrderCommand(Guid OrderIntentId, ...) : ICommand, IIdempotentCommand
///              {
///                  public Guid IdempotencyKey => OrderIntentId;
///              }
///              </code>
/// </summary>
public interface IIdempotentCommand
{
	/// <summary>
	/// Gets a deterministic key that uniquely identifies this command's intent.
	/// Commands with the same key are considered duplicates and will not be re-executed.
	/// Typically derived from a business identifier (e.g., OrderIntentId, PaymentIntentId).
	/// </summary>
	Guid IdempotencyKey { get; }
}
