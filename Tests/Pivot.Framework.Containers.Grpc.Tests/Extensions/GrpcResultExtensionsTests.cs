using FluentAssertions;
using Grpc.Core;
using NSubstitute;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Extensions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Tests.Extensions;

public class GrpcResultExtensionsTests
{
	[Fact]
	public void ThrowIfFailure_WhenResultIsSuccessful_ShouldNotThrow()
	{
		var mapper = Substitute.For<IGrpcResultStatusMapper>();

		var act = () => Result.Success().ThrowIfFailure(mapper);

		act.Should().NotThrow();
	}

	[Fact]
	public void ThrowIfFailure_WhenResultFails_ShouldThrowMappedRpcException()
	{
		var mapper = Substitute.For<IGrpcResultStatusMapper>();
		mapper.Map(Arg.Any<Result>())
			.Returns(new GrpcStatusMapping(StatusCode.NotFound, "order not found"));

		var act = () => Result.Failure(new Error("Orders.NotFound", "order not found"), ResultExceptionType.NotFound)
			.ThrowIfFailure(mapper);

		var exception = act.Should().Throw<RpcException>().Which;
		exception.StatusCode.Should().Be(StatusCode.NotFound);
		exception.Status.Detail.Should().Be("order not found");
	}

	[Fact]
	public void GetValueOrThrow_WhenResultSucceeds_ShouldReturnValue()
	{
		var mapper = Substitute.For<IGrpcResultStatusMapper>();
		var result = Result.Success("pivot");

		var value = result.GetValueOrThrow(mapper);

		value.Should().Be("pivot");
	}
}
