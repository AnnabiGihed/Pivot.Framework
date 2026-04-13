using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pivot.Framework.Authentication.API.Contracts;
using Pivot.Framework.Authentication.API.Extensions;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Services;
using Pivot.Framework.Authentication.Storage;

namespace Pivot.Framework.Authentication.Tests.API;

public class AuthenticationApiTests
{
	#region MapAuthenticationApi Tests
	[Fact]
	public void MapAuthenticationApi_ShouldRegisterExpectedRoutes()
	{
		var builder = WebApplication.CreateBuilder();
		builder.Services.AddAuthenticationApi();
		var app = builder.Build();

		app.MapAuthenticationApi();

		var routes = ((IEndpointRouteBuilder)app).DataSources
			.SelectMany(dataSource => dataSource.Endpoints)
			.OfType<RouteEndpoint>()
			.Select(endpoint => endpoint.RoutePattern.RawText)
			.ToArray();

		routes.Should().Contain("/auth/login");
		routes.Should().Contain("/auth/callback");
		routes.Should().Contain("/auth/refresh");
		routes.Should().Contain("/auth/logout");
		routes.Should().Contain("/auth/profile");
		routes.Should().Contain("/auth/introspect");
	}
	#endregion

	#region Handler Tests
	[Fact]
	public async Task CallbackAsync_ShouldPersistSessionWhenStoreIsAvailable()
	{
		var authService = Substitute.For<IIdentityProviderAuthService>();
		var sessionStore = Substitute.For<IAuthSessionStore>();
		authService.ExchangeAuthorizationCodeAsync(Arg.Any<AuthCodeExchangeRequest>(), Arg.Any<CancellationToken>())
			.Returns(new AuthTokenResponse
			{
				AccessToken = CreateJwt("user-1", "gihed"),
				RefreshToken = "refresh-token",
				ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
			});

		var result = await AuthenticationApiHandlers.CallbackAsync(
			new AuthCodeExchangeRequest { Code = "code-1", RedirectUri = "https://app/callback", SessionId = "session-1" },
			authService,
			sessionStore,
			CancellationToken.None);

		result.Should().NotBeNull();
		await sessionStore.Received(1).SaveAsync(Arg.Is<AuthSession>(session => session.SessionId == "session-1" && session.SubjectId == "user-1"), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ProfileAsync_ShouldReturnUnauthorizedWhenBearerHeaderIsMissing()
	{
		var httpContext = new DefaultHttpContext();
		var authService = Substitute.For<IIdentityProviderAuthService>();

		var result = await AuthenticationApiHandlers.ProfileAsync(httpContext, authService, CancellationToken.None);

		result.Should().NotBeNull();
		await authService.DidNotReceive().GetUserProfileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task IntrospectAsync_ShouldCallService()
	{
		var service = Substitute.For<ITokenIntrospectionService>();
		service.IntrospectTokenAsync("token-1", Arg.Any<CancellationToken>())
			.Returns(new TokenIntrospectionResult { IsActive = true });

		var result = await AuthenticationApiHandlers.IntrospectAsync(new IntrospectTokenRequest { Token = "token-1" }, service, CancellationToken.None);

		result.Should().NotBeNull();
		await service.Received(1).IntrospectTokenAsync("token-1", Arg.Any<CancellationToken>());
	}
	#endregion

	#region Helper Methods
	private static string CreateJwt(string subjectId, string username)
	{
		var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
			claims:
			[
				new Claim("sub", subjectId),
				new Claim("preferred_username", username)
			]);

		return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
	}
	#endregion
}
