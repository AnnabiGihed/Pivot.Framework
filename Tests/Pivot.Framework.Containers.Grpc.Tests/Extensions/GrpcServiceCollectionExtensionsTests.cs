using FluentAssertions;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Containers.Grpc.Extensions;
using Pivot.Framework.Containers.Grpc.Interceptors;
using Pivot.Framework.Containers.Grpc.Tests.TestDoubles;
using Pivot.Framework.Infrastructure.Abstraction.Transaction;

namespace Pivot.Framework.Containers.Grpc.Tests.Extensions;

public class GrpcServiceCollectionExtensionsTests
{
	#region Tests
	[Fact]
	public void AddPivotGrpc_ShouldRegisterCoreMappersAndExceptionInterceptor()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());

		services.AddPivotGrpc();
		using var provider = services.BuildServiceProvider();

		provider.GetRequiredService<IGrpcValidationStatusMapper>().Should().NotBeNull();
		provider.GetRequiredService<IGrpcResultStatusMapper>().Should().NotBeNull();
		provider.GetRequiredService<IGrpcExceptionStatusMapper>().Should().NotBeNull();
		provider.GetRequiredService<GrpcExceptionInterceptor>().Should().NotBeNull();
	}

	[Fact]
	public void AddPivotGrpcTransactions_ShouldRegisterTransactionInterceptor()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
		services.AddSingleton(Substitute.For<ITransactionManager<TestDbContext>>());

		services.AddPivotGrpcTransactions<TestDbContext>();
		using var provider = services.BuildServiceProvider();

		provider.GetRequiredService<GrpcTransactionInterceptor<TestDbContext>>().Should().NotBeNull();
	}

	[Fact]
	public void AddPivotGrpc_ShouldConfigureGrpcOptions()
	{
		var services = new ServiceCollection();

		services.AddPivotGrpc();
		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<GrpcServiceOptions>>().Value;

		options.Interceptors.Should().NotBeEmpty();
	}
	#endregion
}
