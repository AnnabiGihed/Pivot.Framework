namespace Pivot.Framework.Application.Abstractions.Replay;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Provides an ambient replay flag for event replay/projection rebuild scenarios.
///              When <see cref="IsReplaying"/> is true, handlers should suppress all non-projection
///              side effects (email sending, payment processing, external API calls, etc.).
///
///              Usage:
///              - Set by the projection rebuilder or event replay infrastructure.
///              - Checked by event handlers and side-effect-producing services.
///              - Flows across async/await boundaries via <see cref="AsyncLocal{T}"/>.
/// </summary>
public static class ReplayContext
{
	#region Fields

	private static readonly AsyncLocal<bool> _isReplaying = new();

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets whether the current execution context is replaying events.
	/// When true, handlers should only update projections and skip all external side effects.
	/// </summary>
	public static bool IsReplaying
	{
		get => _isReplaying.Value;
		set => _isReplaying.Value = value;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Returns an <see cref="IDisposable"/> scope that sets <see cref="IsReplaying"/> to true
	/// and restores the previous value on disposal.
	/// </summary>
	public static IDisposable BeginReplayScope()
	{
		return new ReplayScope();
	}

	#endregion

	#region Nested Types

	private sealed class ReplayScope : IDisposable
	{
		private readonly bool _previousValue;

		public ReplayScope()
		{
			_previousValue = IsReplaying;
			IsReplaying = true;
		}

		public void Dispose()
		{
			IsReplaying = _previousValue;
		}
	}

	#endregion
}
