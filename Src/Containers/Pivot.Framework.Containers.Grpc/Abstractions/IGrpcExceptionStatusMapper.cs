namespace Pivot.Framework.Containers.Grpc.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Maps application exceptions to transport-safe gRPC status information.
/// </summary>
public interface IGrpcExceptionStatusMapper
{
	#region Methods

	/// <summary>
	/// Maps the supplied exception to a gRPC status payload.
	/// </summary>
	/// <param name="exception">The exception to map.</param>
	/// <returns>The mapped gRPC status.</returns>
	GrpcStatusMapping Map(Exception exception);

	#endregion
}
