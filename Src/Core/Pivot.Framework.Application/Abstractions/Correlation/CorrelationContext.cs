namespace Pivot.Framework.Application.Abstractions.Correlation;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Provides an ambient correlation identifier for end-to-end distributed tracing.
///              The correlation ID flows from the originating HTTP request through the outbox,
///              message broker, and into downstream event handlers via <see cref="AsyncLocal{T}"/>.
///
///              Usage:
///              - Set at the boundary (HTTP middleware, message receiver).
///              - Read by outbox publishers and message publishers to stamp outgoing messages.
///              - Automatically flows across async/await boundaries within the same logical call chain.
/// </summary>
public static class CorrelationContext
{
	#region Fields

	private static readonly AsyncLocal<string?> _correlationId = new();

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets the ambient correlation identifier for the current async flow.
	/// Returns null if no correlation ID has been set.
	/// </summary>
	public static string? CorrelationId
	{
		get => _correlationId.Value;
		set => _correlationId.Value = value;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Returns the current correlation ID, generating a new one if none exists.
	/// Use this at publish boundaries to guarantee a correlation ID is always present.
	/// </summary>
	/// <returns>The current or newly generated correlation identifier.</returns>
	public static string EnsureCorrelationId()
	{
		return _correlationId.Value ??= Guid.NewGuid().ToString();
	}

	#endregion
}
