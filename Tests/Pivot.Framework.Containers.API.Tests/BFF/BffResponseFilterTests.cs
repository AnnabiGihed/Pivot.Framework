using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Pivot.Framework.Containers.API.BFF;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.Tests.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="BffResponseFilter"/>.
///              Verifies that the filter sets 503 status and Retry-After header for unavailable responses,
///              and passes through degraded and full responses unchanged.
/// </summary>
public class BffResponseFilterTests
{
	private readonly BffResponseFilter _sut = new();

	#region Helper Methods

	private static ResultExecutingContext CreateContext(ObjectResult objectResult)
	{
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
		return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), objectResult, controller: null!);
	}

	private static ResultExecutionDelegate CreateDelegate() => () => Task.FromResult<ResultExecutedContext>(null!);

	#endregion

	#region Unavailable Tests

	[Fact]
	public async Task OnResultExecutionAsync_WhenUnavailable_ShouldSet503()
	{
		var bffResponse = BffResponse<string>.Unavailable(30,
			new DegradedComponent { ServiceName = "Svc", AffectedSection = "Section" });
		var objectResult = new ObjectResult(bffResponse);
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().Be(503);
		context.HttpContext.Response.Headers["Retry-After"].ToString().Should().Be("30");
	}

	[Fact]
	public async Task OnResultExecutionAsync_WhenUnavailableWithoutRetryAfter_ShouldSet503WithoutHeader()
	{
		var bffResponse = new BffResponse<string>
		{
			Availability = DataAvailability.Unavailable
		};
		var objectResult = new ObjectResult(bffResponse);
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().Be(503);
		context.HttpContext.Response.Headers.ContainsKey("Retry-After").Should().BeFalse();
	}

	#endregion

	#region Non-Unavailable Tests

	[Fact]
	public async Task OnResultExecutionAsync_WhenFull_ShouldNotModifyStatusCode()
	{
		var bffResponse = BffResponse<string>.Ok("data");
		var objectResult = new ObjectResult(bffResponse);
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().BeNull();
	}

	[Fact]
	public async Task OnResultExecutionAsync_WhenDegraded_ShouldNotModifyStatusCode()
	{
		var bffResponse = BffResponse<string>.Degraded("partial",
			new DegradedComponent { ServiceName = "Svc", AffectedSection = "Section" });
		var objectResult = new ObjectResult(bffResponse);
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().BeNull();
	}

	[Fact]
	public async Task OnResultExecutionAsync_WhenNonBffResponse_ShouldNotModify()
	{
		var objectResult = new ObjectResult("plain string");
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().BeNull();
	}

	[Fact]
	public async Task OnResultExecutionAsync_WhenNullValue_ShouldNotModify()
	{
		var objectResult = new ObjectResult(null);
		var context = CreateContext(objectResult);

		await _sut.OnResultExecutionAsync(context, CreateDelegate());

		objectResult.StatusCode.Should().BeNull();
	}

	#endregion
}
