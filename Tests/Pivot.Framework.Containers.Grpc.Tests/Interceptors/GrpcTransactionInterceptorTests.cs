using FluentAssertions;
using Grpc.Core;
using NSubstitute;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Interceptors;
using Pivot.Framework.Containers.Grpc.Options;
using Pivot.Framework.Containers.Grpc.Tests.TestDoubles;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Containers.Grpc.Tests.Interceptors;

public class GrpcTransactionInterceptorTests
{
	[Fact]
	public async Task UnaryServerHandler_WhenCallSucceeds_ShouldCommit()
	{
		var transactionManager = Substitute.For<ITransactionManager<TestDbContext>>();
		var exceptionMapper = Substitute.For<IGrpcExceptionStatusMapper>();
		var interceptor = new GrpcTransactionInterceptor<TestDbContext>(
			transactionManager,
			exceptionMapper,
			new GrpcTransactionInterceptorOptions());

		var response = await interceptor.UnaryServerHandler(
			"request",
			new TestServerCallContext(),
			static (_, _) => Task.FromResult("ok"));

		response.Should().Be("ok");
		await transactionManager.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
		await transactionManager.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
		await transactionManager.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnaryServerHandler_WhenMappedStatusCommits_ShouldCommitAndRethrow()
	{
		var transactionManager = Substitute.For<ITransactionManager<TestDbContext>>();
		var exceptionMapper = Substitute.For<IGrpcExceptionStatusMapper>();
		exceptionMapper.Map(Arg.Any<Exception>())
			.Returns(new GrpcStatusMapping(StatusCode.InvalidArgument, "validation failed"));

		var interceptor = new GrpcTransactionInterceptor<TestDbContext>(
			transactionManager,
			exceptionMapper,
			new GrpcTransactionInterceptorOptions());

		var act = () => interceptor.UnaryServerHandler<string, string>(
			"request",
			new TestServerCallContext(),
			static (_, _) => throw new InvalidOperationException("boom"));

		await act.Should().ThrowAsync<InvalidOperationException>();
		await transactionManager.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
		await transactionManager.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnaryServerHandler_WhenRpcExceptionIsNotCommitStatus_ShouldRollback()
	{
		var transactionManager = Substitute.For<ITransactionManager<TestDbContext>>();
		var exceptionMapper = Substitute.For<IGrpcExceptionStatusMapper>();

		var interceptor = new GrpcTransactionInterceptor<TestDbContext>(
			transactionManager,
			exceptionMapper,
			new GrpcTransactionInterceptorOptions());

		var act = () => interceptor.UnaryServerHandler<string, string>(
			"request",
			new TestServerCallContext(),
			static (_, _) => throw new RpcException(new Status(StatusCode.PermissionDenied, "denied")));

		await act.Should().ThrowAsync<RpcException>();
		await transactionManager.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
		await transactionManager.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ServerStreamingServerHandler_WhenStreamingInterceptionDisabled_ShouldBypassTransactions()
	{
		var transactionManager = Substitute.For<ITransactionManager<TestDbContext>>();
		var exceptionMapper = Substitute.For<IGrpcExceptionStatusMapper>();

		var interceptor = new GrpcTransactionInterceptor<TestDbContext>(
			transactionManager,
			exceptionMapper,
			new GrpcTransactionInterceptorOptions());

		await interceptor.ServerStreamingServerHandler<string, string>(
			"request",
			Substitute.For<IServerStreamWriter<string>>(),
			new TestServerCallContext(),
			static (_, _, _) => Task.CompletedTask);

		await transactionManager.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
	}
}
