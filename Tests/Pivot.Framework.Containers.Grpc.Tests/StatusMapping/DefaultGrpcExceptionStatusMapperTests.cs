using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Containers.Grpc.StatusMapping;
using Pivot.Framework.Domain.Shared;
using Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

namespace Pivot.Framework.Containers.Grpc.Tests.StatusMapping;

public class DefaultGrpcExceptionStatusMapperTests
{
	#region Tests
	[Fact]
	public void Map_WhenValidationException_ShouldReturnInvalidArgumentWithTrailers()
	{
		var mapper = CreateMapper();
		var errors = new[]
		{
			new Error("schemas.name.required", "Schema name is required.")
		};
		var validationResult = new TestValidationResult { Errors = errors };

		var mapping = mapper.Map(new ValidationException(errors[0], validationResult));

		mapping.StatusCode.Should().Be(StatusCode.InvalidArgument);
		mapping.Trailers.Should().NotBeNull();
		mapping.Trailers!.GetValue("validation-errors").Should().Contain("schemas.name.required");
	}

	[Fact]
	public void Map_WhenNotFoundException_ShouldReturnNotFound()
	{
		var mapper = CreateMapper();

		var mapping = mapper.Map(new NotFoundException("Schema", 42));

		mapping.StatusCode.Should().Be(StatusCode.NotFound);
		mapping.Detail.Should().Be("Schema (42) was not found.");
	}

	[Fact]
	public void Map_WhenUnhandledExceptionInProduction_ShouldHideDetails()
	{
		var mapper = CreateMapper(Environments.Production);

		var mapping = mapper.Map(new InvalidOperationException("boom"));

		mapping.StatusCode.Should().Be(StatusCode.Internal);
		mapping.Detail.Should().Be("An unexpected error occurred.");
	}
	#endregion

	#region Helpers
	private static DefaultGrpcExceptionStatusMapper CreateMapper(string? environmentName = null)
	{
		return new DefaultGrpcExceptionStatusMapper(
			new TestHostEnvironment { EnvironmentName = environmentName ?? Environments.Development },
			new DefaultGrpcValidationStatusMapper());
	}
	#endregion
}
