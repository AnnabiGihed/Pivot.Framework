namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration options for the outbox drain process, including the mode of operation and polling interval.
/// </summary>
public sealed class OutboxDrainOptions
{
    #region Properties
    /// <summary>
    /// The drain mode that determines how outbox messages are processed.
    /// </summary>
    public OutboxDrainMode Mode { get; set; }

    /// <summary>
    /// The interval at which the background polling service checks for pending outbox messages.
    /// Only relevant when <see cref="Mode"/> is <see cref="OutboxDrainMode.BackgroundPolling"/>.
    /// Defaults to 5 seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    #endregion
}