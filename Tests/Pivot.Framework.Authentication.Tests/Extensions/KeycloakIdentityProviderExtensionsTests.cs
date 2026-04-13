using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Authentication.AspNetCore.Extensions;
using Pivot.Framework.Authentication.Services;
using Pivot.Framework.Authentication.Storage;

namespace Pivot.Framework.Authentication.Tests.Extensions;

public class KeycloakIdentityProviderExtensionsTests
{
	#region Registration Tests
	[Fact]
	public void AddKeycloakIdentityProviderServices_ShouldRegisterBackendServices()
	{
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Keycloak:BaseUrl"] = "https://auth.example.com",
				["Keycloak:Realm"] = "pivot",
				["Keycloak:ClientId"] = "pivot-api",
				["Keycloak:ClientSecret"] = "secret",
				["Keycloak:Audience"] = "pivot-api"
			})
			.Build();

		services.AddKeycloakIdentityProviderServices(configuration);
		services.AddInMemoryAuthSessions();

		services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IIdentityProviderAuthService));
		services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IIdentityProviderAdminService));
		services.Should().Contain(descriptor => descriptor.ServiceType == typeof(ITokenIntrospectionService));
		services.Should().Contain(descriptor => descriptor.ServiceType == typeof(ITokenRevocationService));
		services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IAuthSessionStore));
	}
	#endregion
}
