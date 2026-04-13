using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Services;
using Pivot.Framework.Authentication.Tests.TestDoubles;

namespace Pivot.Framework.Authentication.Tests.Services;

public class KeycloakIdentityProviderAuthServiceTests
{
	#region Test Infrastructure
	private static readonly KeycloakOptions Settings = new()
	{
		BaseUrl = "https://auth.example.com",
		Realm = "pivot",
		ClientId = "pivot-api",
		ClientSecret = "secret",
		Audience = "pivot-api"
	};
	#endregion

	#region Tests
	[Fact]
	public async Task BuildAuthorizationUrlAsync_ShouldIncludeExpectedParameters()
	{
		var service = CreateService(new StubHttpMessageHandler());

		var result = await service.BuildAuthorizationUrlAsync(new AuthAuthorizationRequest
		{
			RedirectUri = "https://app.example.com/callback",
			State = "state-1",
			CodeChallenge = "challenge"
		});

		result.AuthorizationUrl.Should().Contain("client_id=pivot-api");
		result.AuthorizationUrl.Should().Contain(Uri.EscapeDataString("https://app.example.com/callback"));
		result.AuthorizationUrl.Should().Contain("code_challenge=challenge");
	}

	[Fact]
	public async Task ExchangeAuthorizationCodeAsync_ShouldPostAuthorizationCodeGrant()
	{
		var handler = new StubHttpMessageHandler();
		handler.EnqueueJson("""
		{
		  "access_token": "access-token",
		  "refresh_token": "refresh-token",
		  "id_token": "id-token",
		  "token_type": "Bearer",
		  "scope": "openid profile",
		  "expires_in": 60,
		  "refresh_expires_in": 120
		}
		""");

		var service = CreateService(handler);

		var result = await service.ExchangeAuthorizationCodeAsync(new AuthCodeExchangeRequest
		{
			Code = "code-1",
			RedirectUri = "https://app.example.com/callback",
			CodeVerifier = "verifier"
		});

		result.AccessToken.Should().Be("access-token");
		result.RefreshToken.Should().Be("refresh-token");
		var requestBody = await handler.Requests.Single().Content!.ReadAsStringAsync();
		requestBody.Should().Contain("grant_type=authorization_code");
		requestBody.Should().Contain("code=code-1");
		requestBody.Should().Contain("code_verifier=verifier");
	}

	[Fact]
	public async Task GetUserProfileAsync_ShouldMapUserInfoPayload()
	{
		var handler = new StubHttpMessageHandler();
		handler.EnqueueJson("""
		{
		  "sub": "user-1",
		  "preferred_username": "gihed",
		  "email": "gihed@example.com",
		  "given_name": "Gihed",
		  "family_name": "Annabi",
		  "name": "Gihed Annabi"
		}
		""");

		var service = CreateService(handler);

		var result = await service.GetUserProfileAsync("access-token");

		result.Id.Should().Be("user-1");
		result.Username.Should().Be("gihed");
		result.Email.Should().Be("gihed@example.com");
		handler.Requests.Single().Headers.Authorization!.Scheme.Should().Be("Bearer");
	}
	#endregion

	#region Helpers
	private static KeycloakIdentityProviderAuthService CreateService(StubHttpMessageHandler handler)
	{
		return new KeycloakIdentityProviderAuthService(
			new HttpClient(handler),
			Microsoft.Extensions.Options.Options.Create(Settings));
	}
	#endregion
}
