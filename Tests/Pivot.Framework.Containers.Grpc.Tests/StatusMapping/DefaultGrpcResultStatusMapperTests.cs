using FluentAssertions;
using Grpc.Core;
using Pivot.Framework.Containers.Grpc.StatusMapping;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Tests.StatusMapping;

public class DefaultGrpcResultStatusMapperTests
{
	private readonly DefaultGrpcResultStatusMapper _mapper = new();

	[Theory]
	[InlineData(ResultExceptionType.ValidationError, StatusCode.InvalidArgument)]
	[InlineData(ResultExceptionType.NotFound, StatusCode.NotFound)]
	[InlineData(ResultExceptionType.Conflict, StatusCode.AlreadyExists)]
	[InlineData(ResultExceptionType.AuthenticationRequired, StatusCode.Unauthenticated)]
	[InlineData(ResultExceptionType.AccessDenied, StatusCode.PermissionDenied)]
	public void Map_ShouldTranslateFailureType(ResultExceptionType resultExceptionType, StatusCode expectedStatusCode)
	{
		var result = Result.Failure(new Error("test", "failed"), resultExceptionType);

		var mapping = _mapper.Map(result);

		mapping.StatusCode.Should().Be(expectedStatusCode);
		mapping.Detail.Should().Be("failed");
	}

	[Fact]
	public void Map_WhenResultIsSuccessful_ShouldThrow()
	{
		var act = () => _mapper.Map(Result.Success());

		act.Should().Throw<InvalidOperationException>();
	}
}
