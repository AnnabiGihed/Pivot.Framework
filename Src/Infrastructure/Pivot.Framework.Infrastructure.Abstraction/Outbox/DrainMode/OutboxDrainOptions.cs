namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Configuration options for the outbox drain process, including the mode of operation and polling interval.
/// </summary>
public sealed class OutboxDrainOptions
{
    public OutboxDrainMode Mode { get; set; }
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}