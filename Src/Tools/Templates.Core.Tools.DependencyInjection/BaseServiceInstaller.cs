using Scrutor;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Templates.Core.Tools.DependencyInjection;

public abstract class BaseServiceInstaller
{


	protected void IncludeConventionBasedRegistrations(IServiceCollection services, IConfiguration configuration, Assembly[] assemblies)
	{
		// for allconvention based registrations, should e.g. register automatically next : OnlineEducationEmailService : IOnlineEducationEmailService
		services
		.Scan(
			selector => selector
				.FromAssemblies(assemblies)
					.AddClasses(false)
					.UsingRegistrationStrategy(RegistrationStrategy.Skip)
					.AsMatchingInterface()
					.WithScopedLifetime());
	}
}
