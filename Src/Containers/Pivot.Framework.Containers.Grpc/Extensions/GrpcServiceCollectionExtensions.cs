using Grpc.AspNetCore.Server;
using Grpc.Core.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Interceptors;
using Pivot.Framework.Containers.Grpc.Options;
using Pivot.Framework.Containers.Grpc.StatusMapping;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;

namespace Pivot.Framework.Containers.Grpc.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : DI registration helpers for Pivot gRPC services.
/// </summary>
public static class GrpcServiceCollectionExtensions
{
	#region Public Methods

	/// <summary>
	/// Registers the framework gRPC stack, including exception mapping and transport mappers.
	/// </summary>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="configureGrpc">Optional callback for additional gRPC server options.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddPivotGrpc(
		this IServiceCollection services,
		Action<GrpcServiceOptions>? configureGrpc = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddGrpc(options =>
		{
			options.Interceptors.Add<GrpcExceptionInterceptor>();
			configureGrpc?.Invoke(options);
		});

		services.TryAddSingleton<IGrpcValidationStatusMapper, DefaultGrpcValidationStatusMapper>();
		services.TryAddSingleton<IGrpcResultStatusMapper, DefaultGrpcResultStatusMapper>();
		services.TryAddSingleton<IGrpcExceptionStatusMapper, DefaultGrpcExceptionStatusMapper>();
		services.TryAddScoped<GrpcExceptionInterceptor>();

		return services;
	}

	/// <summary>
	/// Registers a transaction interceptor for gRPC request handlers backed by the given persistence context.
	/// </summary>
	/// <typeparam name="TContext">The persistence context participating in the transaction boundary.</typeparam>
	/// <param name="services">The service collection to register into.</param>
	/// <param name="configure">Optional callback for transaction interceptor behaviour.</param>
	/// <returns>The same <paramref name="services"/> instance for chaining.</returns>
	public static IServiceCollection AddPivotGrpcTransactions<TContext>(
		this IServiceCollection services,
		Action<GrpcTransactionInterceptorOptions>? configure = null)
		where TContext : DbContext, IPersistenceContext
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddPivotGrpc();

		var options = new GrpcTransactionInterceptorOptions();
		configure?.Invoke(options);

		services.TryAddSingleton(options);
		services.TryAddScoped<GrpcTransactionInterceptor<TContext>>();
		services.AddGrpc(grpcOptions => grpcOptions.Interceptors.Add<GrpcTransactionInterceptor<TContext>>());

		return services;
	}

	#endregion
}
