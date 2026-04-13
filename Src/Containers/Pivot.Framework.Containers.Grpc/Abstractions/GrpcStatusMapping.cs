using Grpc.Core;

namespace Pivot.Framework.Containers.Grpc.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Represents a mapped gRPC status together with optional response trailers.
/// </summary>
public sealed record GrpcStatusMapping
{
	#region Properties
	/// <summary>
	/// The gRPC status code to return to the caller.
	/// </summary>
	public StatusCode StatusCode { get; init; }

	/// <summary>
	/// The transport-safe detail message.
	/// </summary>
	public string Detail { get; init; }

	/// <summary>
	/// Optional trailers carrying structured metadata.
	/// </summary>
	public Metadata? Trailers { get; init; }
	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of <see cref="GrpcStatusMapping"/>.
	/// </summary>
	/// <param name="statusCode">The gRPC status code to return to the caller.</param>
	/// <param name="detail">The transport-safe detail message.</param>
	/// <param name="trailers">Optional trailers carrying structured metadata.</param>
	public GrpcStatusMapping(StatusCode statusCode, string detail, Metadata? trailers = null)
	{
		StatusCode = statusCode;
		Detail = detail;
		Trailers = trailers;
	}
	#endregion
}
