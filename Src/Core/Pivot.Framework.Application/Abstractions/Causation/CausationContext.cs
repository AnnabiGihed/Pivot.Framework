namespace Pivot.Framework.Application.Abstractions.Causation;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Provides an ambient causation identifier for event chain tracing.
///              When a handler processes event A and produces event B, B's CausationId = A's EventId.
///              This enables reconstructing the full causal chain of events in a distributed system.
///
///              Usage:
///              - Set by the event consumer/handler infrastructure when processing an incoming event.
///              - Read by event publishers to stamp the CausationId on outgoing events.
///              - Flows across async/await boundaries via <see cref="AsyncLocal{T}"/>.
/// </summary>
public static class CausationContext
{
	#region Fields

	private static readonly AsyncLocal<string?> _causationId = new();

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets the ambient causation identifier for the current async flow.
	/// Represents the EventId of the event that caused the current processing.
	/// Returns null if no causation context has been set (e.g., originates from a user action).
	/// </summary>
	public static string? CausationId
	{
		get => _causationId.Value;
		set => _causationId.Value = value;
	}

	#endregion
}
