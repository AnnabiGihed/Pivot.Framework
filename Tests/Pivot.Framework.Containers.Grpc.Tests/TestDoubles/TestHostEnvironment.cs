using Microsoft.Extensions.Hosting;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

/// <summary>
/// Minimal host environment test double.
/// </summary>
internal sealed class TestHostEnvironment : IHostEnvironment
{
	#region Properties
	/// <summary>
	/// Environment name for the test host.
	/// </summary>
	public string EnvironmentName { get; set; } = Environments.Development;

	/// <summary>
	/// Application name for the test host.
	/// </summary>
	public string ApplicationName { get; set; } = "Pivot.Framework.Tests";

	/// <summary>
	/// Content root path for the test host.
	/// </summary>
	public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

	/// <summary>
	/// Content root file provider for the test host.
	/// </summary>
	public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
	#endregion
}
