using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Maps <see cref="Result"/> failures to gRPC status information.
/// </summary>
public interface IGrpcResultStatusMapper
{
	#region Methods

	/// <summary>
	/// Maps the supplied result failure to a gRPC status payload.
	/// </summary>
	/// <param name="result">The failed result to map.</param>
	/// <returns>The mapped gRPC status.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the supplied result is successful.</exception>
	GrpcStatusMapping Map(Result result);

	#endregion
}
