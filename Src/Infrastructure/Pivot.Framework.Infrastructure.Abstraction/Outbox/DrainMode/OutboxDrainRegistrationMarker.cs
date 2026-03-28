namespace Pivot.Framework.Infrastructure.Abstraction.Outbox.DrainMode;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Marker class used to indicate that the outbox drain process has been registered in the dependency injection container, along with the mode of operation.
/// </summary>
public sealed class OutboxDrainRegistrationMarker
{
    #region Properties
    /// <summary>
    /// The outbox drain mode that was registered.
    /// </summary>
    public OutboxDrainMode Mode { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initialises a new instance of <see cref="OutboxDrainRegistrationMarker"/> with the specified drain mode.
    /// </summary>
    /// <param name="mode">The outbox drain mode that was registered.</param>
    public OutboxDrainRegistrationMarker(OutboxDrainMode mode)
    {
        Mode = mode;
    }
    #endregion
}