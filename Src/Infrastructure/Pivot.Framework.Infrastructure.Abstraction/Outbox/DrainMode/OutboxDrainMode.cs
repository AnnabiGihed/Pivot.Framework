namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Enumeration of supported outbox drain modes.
/// </summary>
public enum OutboxDrainMode
{
    /// <summary>
    /// The outbox is drained synchronously immediately after each HTTP request completes.
    /// </summary>
    ImmediateAfterRequest = 1,

    /// <summary>
    /// The outbox is drained by a background polling service at a configurable interval.
    /// </summary>
    BackgroundPolling = 2
}