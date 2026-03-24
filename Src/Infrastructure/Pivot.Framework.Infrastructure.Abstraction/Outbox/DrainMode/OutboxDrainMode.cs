namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Enumeration of supported outbox drain modes.
/// </summary>
public enum OutboxDrainMode
{
    ImmediateAfterRequest = 1,
    BackgroundPolling = 2
}