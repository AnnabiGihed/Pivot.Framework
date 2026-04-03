using Microsoft.Extensions.Hosting;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

internal sealed class TestHostEnvironment : IHostEnvironment
{
	public string EnvironmentName { get; set; } = Environments.Development;
	public string ApplicationName { get; set; } = "Pivot.Framework.Tests";
	public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
	public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
}
