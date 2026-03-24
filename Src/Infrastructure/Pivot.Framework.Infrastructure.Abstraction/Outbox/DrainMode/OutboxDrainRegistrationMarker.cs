namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Marker class used to indicate that the outbox drain process has been registered in the dependency injection container, along with the mode of operation.
/// </summary>
public sealed class OutboxDrainRegistrationMarker
{
    public OutboxDrainMode Mode { get; }

    public OutboxDrainRegistrationMarker(OutboxDrainMode mode)
    {
        Mode = mode;
    }
}