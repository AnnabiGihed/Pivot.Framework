using Grpc.Core;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.StatusMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Maps <see cref="Result"/> failures to gRPC status information.
/// </summary>
public sealed class DefaultGrpcResultStatusMapper : IGrpcResultStatusMapper
{
	#region Methods

	/// <inheritdoc />
	public GrpcStatusMapping Map(Result result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (result.IsSuccess)
			throw new InvalidOperationException("Only failed results can be mapped to gRPC status.");

		var detail = string.IsNullOrWhiteSpace(result.Error.Message)
			? "The request could not be completed."
			: result.Error.Message;

		return result.ResultExceptionType switch
		{
			ResultExceptionType.ValidationError => new GrpcStatusMapping(StatusCode.InvalidArgument, detail),
			ResultExceptionType.NotFound => new GrpcStatusMapping(StatusCode.NotFound, detail),
			ResultExceptionType.Conflict => new GrpcStatusMapping(StatusCode.AlreadyExists, detail),
			ResultExceptionType.AuthenticationRequired => new GrpcStatusMapping(StatusCode.Unauthenticated, detail),
			ResultExceptionType.AccessDenied => new GrpcStatusMapping(StatusCode.PermissionDenied, detail),
			_ => new GrpcStatusMapping(StatusCode.FailedPrecondition, detail)
		};
	}

	#endregion
}
