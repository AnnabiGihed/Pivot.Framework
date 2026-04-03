using Grpc.Core;

namespace Pivot.Framework.Containers.Grpc.Abstractions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Represents a mapped gRPC status together with optional response trailers.
/// </summary>
/// <param name="StatusCode">The gRPC status code to return to the caller.</param>
/// <param name="Detail">The transport-safe detail message.</param>
/// <param name="Trailers">Optional trailers carrying structured metadata.</param>
public sealed record GrpcStatusMapping(StatusCode StatusCode, string Detail, Metadata? Trailers = null);
