using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Templates.Core.Tools.DependencyInjection.Abstractions;

public interface IServiceInstaller
{
	void Install(IServiceCollection services, IConfiguration configuration, bool includeConventionBasedRegistration = true);
}