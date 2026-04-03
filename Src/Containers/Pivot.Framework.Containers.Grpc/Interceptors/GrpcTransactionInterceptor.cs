using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.EntityFrameworkCore;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Options;
using Pivot.Framework.Infrastructure.Abstraction.Persistence;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Containers.Grpc.Interceptors;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Owns the transaction boundary for gRPC request handlers.
///              By default only unary calls are wrapped to avoid long-lived transactions for streams.
/// </summary>
public sealed class GrpcTransactionInterceptor<TContext> : Interceptor
	where TContext : DbContext, IPersistenceContext
{
	#region Fields

	private readonly IGrpcExceptionStatusMapper _exceptionStatusMapper;
	private readonly GrpcTransactionInterceptorOptions _options;
	private readonly ITransactionManager<TContext> _transactionManager;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new interceptor with the required transaction infrastructure.
	/// </summary>
	/// <param name="transactionManager">The transaction manager owning the database transaction.</param>
	/// <param name="exceptionStatusMapper">The mapper used to classify exception outcomes.</param>
	/// <param name="options">The transaction interceptor options.</param>
	public GrpcTransactionInterceptor(
		ITransactionManager<TContext> transactionManager,
		IGrpcExceptionStatusMapper exceptionStatusMapper,
		GrpcTransactionInterceptorOptions options)
	{
		_transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
		_exceptionStatusMapper = exceptionStatusMapper ?? throw new ArgumentNullException(nameof(exceptionStatusMapper));
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	#endregion

	#region Unary

	/// <inheritdoc />
	public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
		TRequest request,
		ServerCallContext context,
		UnaryServerMethod<TRequest, TResponse> continuation)
		=> _options.InterceptUnaryCalls
			? ExecuteInTransactionAsync(() => continuation(request, context), context.CancellationToken)
			: continuation(request, context);

	#endregion

	#region Client Streaming

	/// <inheritdoc />
	public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		ServerCallContext context,
		ClientStreamingServerMethod<TRequest, TResponse> continuation)
		=> _options.InterceptClientStreamingCalls
			? ExecuteInTransactionAsync(() => continuation(requestStream, context), context.CancellationToken)
			: continuation(requestStream, context);

	#endregion

	#region Server Streaming

	/// <inheritdoc />
	public override Task ServerStreamingServerHandler<TRequest, TResponse>(
		TRequest request,
		IServerStreamWriter<TResponse> responseStream,
		ServerCallContext context,
		ServerStreamingServerMethod<TRequest, TResponse> continuation)
		=> _options.InterceptServerStreamingCalls
			? ExecuteInTransactionAsync(() => continuation(request, responseStream, context), context.CancellationToken)
			: continuation(request, responseStream, context);

	#endregion

	#region Duplex Streaming

	/// <inheritdoc />
	public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		IServerStreamWriter<TResponse> responseStream,
		ServerCallContext context,
		DuplexStreamingServerMethod<TRequest, TResponse> continuation)
		=> _options.InterceptDuplexStreamingCalls
			? ExecuteInTransactionAsync(() => continuation(requestStream, responseStream, context), context.CancellationToken)
			: continuation(requestStream, responseStream, context);

	#endregion

	#region Private Helpers

	private async Task<TResponse> ExecuteInTransactionAsync<TResponse>(
		Func<Task<TResponse>> continuation,
		CancellationToken cancellationToken)
	{
		await _transactionManager.BeginTransactionAsync(cancellationToken);

		try
		{
			var response = await continuation();
			await _transactionManager.CommitTransactionAsync(cancellationToken);
			return response;
		}
		catch (Exception exception)
		{
			var statusCode = exception is RpcException rpcException
				? rpcException.StatusCode
				: _exceptionStatusMapper.Map(exception).StatusCode;

			if (_options.ShouldCommitStatusCode(statusCode))
			{
				await _transactionManager.CommitTransactionAsync(cancellationToken);
			}
			else
			{
				await _transactionManager.RollbackTransactionAsync(cancellationToken);
			}

			throw;
		}
	}

	private async Task ExecuteInTransactionAsync(
		Func<Task> continuation,
		CancellationToken cancellationToken)
	{
		await _transactionManager.BeginTransactionAsync(cancellationToken);

		try
		{
			await continuation();
			await _transactionManager.CommitTransactionAsync(cancellationToken);
		}
		catch (Exception exception)
		{
			var statusCode = exception is RpcException rpcException
				? rpcException.StatusCode
				: _exceptionStatusMapper.Map(exception).StatusCode;

			if (_options.ShouldCommitStatusCode(statusCode))
			{
				await _transactionManager.CommitTransactionAsync(cancellationToken);
			}
			else
			{
				await _transactionManager.RollbackTransactionAsync(cancellationToken);
			}

			throw;
		}
	}

	#endregion
}
