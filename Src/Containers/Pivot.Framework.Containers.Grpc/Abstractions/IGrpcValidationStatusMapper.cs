using Grpc.Core;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Serialises validation errors into gRPC trailers.
/// </summary>
public interface IGrpcValidationStatusMapper
{
	#region Methods

	/// <summary>
	/// Creates gRPC trailers that carry validation error details.
	/// </summary>
	/// <param name="errors">The validation errors to serialise.</param>
	/// <returns>A metadata collection containing validation details.</returns>
	Metadata CreateTrailers(IReadOnlyCollection<Error> errors);

	#endregion
}
