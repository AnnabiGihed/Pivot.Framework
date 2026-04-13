using FluentAssertions;
using Pivot.Framework.Authentication.AspNetCore.Testing;

namespace Pivot.Framework.Authentication.Tests.Testing;

public class AuthenticationTestContextFactoryTests
{
	#region Tests
	[Fact]
	public void CreatePrincipal_ShouldIncludeRolesAndClaims()
	{
		var principal = AuthenticationTestContextFactory.CreatePrincipal(
			subjectId: Guid.NewGuid().ToString(),
			username: "gihed",
			email: "gihed@example.com",
			roles: ["admin"]);

		principal.Identity!.IsAuthenticated.Should().BeTrue();
		principal.IsInRole("admin").Should().BeTrue();
		principal.FindFirst("preferred_username")!.Value.Should().Be("gihed");
	}

	[Fact]
	public void CreateHttpContext_ShouldAttachPrincipal()
	{
		var principal = AuthenticationTestContextFactory.CreatePrincipal(username: "gihed");

		var context = AuthenticationTestContextFactory.CreateHttpContext(principal);

		context.User.Should().BeSameAs(principal);
	}
	#endregion
}
