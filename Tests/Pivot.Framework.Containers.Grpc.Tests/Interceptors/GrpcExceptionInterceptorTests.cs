using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Interceptors;
using Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

namespace Pivot.Framework.Containers.Grpc.Tests.Interceptors;

public class GrpcExceptionInterceptorTests
{
	#region Tests
	[Fact]
	public async Task UnaryServerHandler_WhenExceptionIsThrown_ShouldMapToRpcException()
	{
		var mapper = Substitute.For<IGrpcExceptionStatusMapper>();
		mapper.Map(Arg.Any<Exception>())
			.Returns(new GrpcStatusMapping(StatusCode.InvalidArgument, "validation failed"));

		var interceptor = new GrpcExceptionInterceptor(mapper, NullLogger<GrpcExceptionInterceptor>.Instance);

		var act = () => interceptor.UnaryServerHandler<string, string>(
			"request",
			new TestServerCallContext(),
			static (_, _) => throw new InvalidOperationException("boom"));

		var exception = await act.Should().ThrowAsync<RpcException>();
		exception.Which.StatusCode.Should().Be(StatusCode.InvalidArgument);
	}
	#endregion
}
