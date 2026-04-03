using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Pivot.Framework.Containers.Grpc.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Framework-friendly endpoint mapping helpers for gRPC services.
/// </summary>
public static class GrpcEndpointRouteBuilderExtensions
{
	#region Methods

	/// <summary>
	/// Maps a gRPC service onto the current endpoint route builder.
	/// </summary>
	/// <typeparam name="TService">The gRPC service implementation type.</typeparam>
	/// <param name="endpoints">The endpoint route builder.</param>
	/// <returns>The convention builder for the mapped service.</returns>
	public static GrpcServiceEndpointConventionBuilder MapPivotGrpcService<TService>(this IEndpointRouteBuilder endpoints)
		where TService : class
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		return endpoints.MapGrpcService<TService>();
	}

	#endregion
}
