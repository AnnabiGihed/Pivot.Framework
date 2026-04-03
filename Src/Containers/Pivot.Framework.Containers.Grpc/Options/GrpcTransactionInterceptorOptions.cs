using Grpc.Core;

namespace Pivot.Framework.Containers.Grpc.Options;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Controls which gRPC call types participate in the transaction boundary
///              and which status codes commit the transaction.
/// </summary>
public sealed class GrpcTransactionInterceptorOptions
{
	#region Constructors

	/// <summary>
	/// Initialises the options with safe defaults for request/response command handling.
	/// </summary>
	public GrpcTransactionInterceptorOptions()
	{
		ShouldCommitStatusCode = static statusCode => statusCode is StatusCode.OK or StatusCode.InvalidArgument;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets whether unary calls are wrapped in a transaction. Enabled by default.
	/// </summary>
	public bool InterceptUnaryCalls { get; set; } = true;

	/// <summary>
	/// Gets or sets whether client streaming calls are wrapped in a transaction.
	/// Disabled by default to avoid holding transactions open during long-running streams.
	/// </summary>
	public bool InterceptClientStreamingCalls { get; set; }

	/// <summary>
	/// Gets or sets whether server streaming calls are wrapped in a transaction.
	/// Disabled by default to avoid holding transactions open during long-running streams.
	/// </summary>
	public bool InterceptServerStreamingCalls { get; set; }

	/// <summary>
	/// Gets or sets whether duplex streaming calls are wrapped in a transaction.
	/// Disabled by default to avoid holding transactions open during long-running streams.
	/// </summary>
	public bool InterceptDuplexStreamingCalls { get; set; }

	/// <summary>
	/// Gets or sets the predicate that determines whether a transaction should be committed
	/// for a completed gRPC call.
	/// </summary>
	public Func<StatusCode, bool> ShouldCommitStatusCode { get; set; }

	#endregion
}
