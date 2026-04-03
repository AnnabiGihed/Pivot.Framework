using Grpc.Core;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Convenience helpers for translating Pivot results into gRPC failures.
/// </summary>
public static class GrpcResultExtensions
{
	#region Methods

	/// <summary>
	/// Throws a <see cref="RpcException"/> when the supplied result is a failure.
	/// </summary>
	/// <param name="result">The result to validate.</param>
	/// <param name="resultStatusMapper">The mapper used to convert failures to gRPC status information.</param>
	public static void ThrowIfFailure(this Result result, IGrpcResultStatusMapper resultStatusMapper)
	{
		ArgumentNullException.ThrowIfNull(result);
		ArgumentNullException.ThrowIfNull(resultStatusMapper);

		if (result.IsSuccess)
			return;

		var mapping = resultStatusMapper.Map(result);
		throw new RpcException(new Status(mapping.StatusCode, mapping.Detail), mapping.Trailers ?? []);
	}

	/// <summary>
	/// Returns the result value when successful, otherwise throws a mapped <see cref="RpcException"/>.
	/// </summary>
	/// <typeparam name="TValue">The result value type.</typeparam>
	/// <param name="result">The result to unwrap.</param>
	/// <param name="resultStatusMapper">The mapper used to convert failures to gRPC status information.</param>
	/// <returns>The successful result value.</returns>
	public static TValue GetValueOrThrow<TValue>(this Result<TValue> result, IGrpcResultStatusMapper resultStatusMapper)
	{
		ArgumentNullException.ThrowIfNull(result);

		result.ThrowIfFailure(resultStatusMapper);
		return result.Value;
	}

	#endregion
}
