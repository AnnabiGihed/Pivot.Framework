using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Authentication.AspNetCore.Extensions;

namespace Pivot.Framework.Authentication.Tests.Extensions;

public class AuthorizationPolicyExtensionsTests
{
	#region Policy Tests
	[Fact]
	public async Task AddRolePolicy_ShouldAuthorizeUserWithRole()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddAuthorization(options => options.AddRolePolicy("admin", "admin"));
		using var provider = services.BuildServiceProvider();

		var authorizationService = provider.GetRequiredService<IAuthorizationService>();
		var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "admin")], "test"));

		var result = await authorizationService.AuthorizeAsync(principal, null, "admin");

		result.Succeeded.Should().BeTrue();
	}

	[Fact]
	public async Task AddScopePolicy_ShouldRequireAllScopes()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddAuthorization(options => options.AddScopePolicy("write-schemas", "schema.write", "schema.read"));
		using var provider = services.BuildServiceProvider();

		var authorizationService = provider.GetRequiredService<IAuthorizationService>();
		var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("scope", "schema.read schema.write")], "test"));

		var result = await authorizationService.AuthorizeAsync(principal, null, "write-schemas");

		result.Succeeded.Should().BeTrue();
	}
	#endregion
}
