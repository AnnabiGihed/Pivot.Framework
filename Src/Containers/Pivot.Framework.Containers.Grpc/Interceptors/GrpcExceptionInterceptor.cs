using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Pivot.Framework.Containers.Grpc.Abstractions;

namespace Pivot.Framework.Containers.Grpc.Interceptors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Global gRPC exception interceptor that converts unhandled exceptions
///              into transport-safe <see cref="RpcException"/> instances.
/// </summary>
public sealed class GrpcExceptionInterceptor : Interceptor
{
	#region Fields

	private readonly IGrpcExceptionStatusMapper _exceptionStatusMapper;
	private readonly ILogger<GrpcExceptionInterceptor> _logger;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new interceptor with the required mapper and logger.
	/// </summary>
	/// <param name="exceptionStatusMapper">The exception mapper used to build gRPC failures.</param>
	/// <param name="logger">The diagnostic logger.</param>
	public GrpcExceptionInterceptor(
		IGrpcExceptionStatusMapper exceptionStatusMapper,
		ILogger<GrpcExceptionInterceptor> logger)
	{
		_exceptionStatusMapper = exceptionStatusMapper ?? throw new ArgumentNullException(nameof(exceptionStatusMapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region Unary

	/// <inheritdoc />
	public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
		TRequest request,
		ServerCallContext context,
		UnaryServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			return await continuation(request, context);
		}
		catch (Exception exception)
		{
			throw MapToRpcException(exception);
		}
	}

	#endregion

	#region Client Streaming

	/// <inheritdoc />
	public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		ServerCallContext context,
		ClientStreamingServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			return await continuation(requestStream, context);
		}
		catch (Exception exception)
		{
			throw MapToRpcException(exception);
		}
	}

	#endregion

	#region Server Streaming

	/// <inheritdoc />
	public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
		TRequest request,
		IServerStreamWriter<TResponse> responseStream,
		ServerCallContext context,
		ServerStreamingServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			await continuation(request, responseStream, context);
		}
		catch (Exception exception)
		{
			throw MapToRpcException(exception);
		}
	}

	#endregion

	#region Duplex Streaming

	/// <inheritdoc />
	public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		IServerStreamWriter<TResponse> responseStream,
		ServerCallContext context,
		DuplexStreamingServerMethod<TRequest, TResponse> continuation)
	{
		try
		{
			await continuation(requestStream, responseStream, context);
		}
		catch (Exception exception)
		{
			throw MapToRpcException(exception);
		}
	}

	#endregion

	#region Private Helpers

	private RpcException MapToRpcException(Exception exception)
	{
		var mapping = _exceptionStatusMapper.Map(exception);

		_logger.LogError(
			exception,
			"Unhandled gRPC exception mapped to {StatusCode}. Method: {Method}.",
			mapping.StatusCode,
			exception is RpcException ? "grpc" : "application");

		return new RpcException(new Status(mapping.StatusCode, mapping.Detail), mapping.Trailers ?? []);
	}

	#endregion
}
