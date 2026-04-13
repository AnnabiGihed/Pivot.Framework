using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Services;
using Pivot.Framework.Authentication.Tests.TestDoubles;

namespace Pivot.Framework.Authentication.Tests.Services;

public class KeycloakManagementServicesTests
{
	#region Test Infrastructure
	private static readonly KeycloakOptions Settings = new()
	{
		BaseUrl = "https://auth.example.com",
		Realm = "pivot",
		ClientId = "pivot-api",
		ClientSecret = "secret",
		AdminClientId = "pivot-admin",
		AdminClientSecret = "admin-secret",
		Audience = "pivot-api"
	};
	#endregion

	#region Tests
	[Fact]
	public async Task IntrospectTokenAsync_ShouldMapKeycloakPayload()
	{
		var handler = new StubHttpMessageHandler();
		handler.EnqueueJson("""
		{
		  "active": true,
		  "sub": "user-1",
		  "username": "gihed",
		  "client_id": "pivot-api",
		  "scope": "openid profile",
		  "exp": 4102444800
		}
		""");

		var service = new KeycloakTokenIntrospectionService(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(Settings));

		var result = await service.IntrospectTokenAsync("token-1");

		result.IsActive.Should().BeTrue();
		result.SubjectId.Should().Be("user-1");
		result.Scopes.Should().Contain(["openid", "profile"]);
	}

	[Fact]
	public async Task RevokeTokenAsync_ShouldPostTokenToRevocationEndpoint()
	{
		var handler = new StubHttpMessageHandler();
		handler.Enqueue(new HttpResponseMessage(HttpStatusCode.OK));
		var service = new KeycloakTokenRevocationService(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(Settings));

		await service.RevokeTokenAsync("token-1", "refresh_token");

		var request = handler.Requests.Single();
		request.RequestUri!.AbsoluteUri.Should().Be(Settings.RevocationUrl);
		var body = await request.Content!.ReadAsStringAsync();
		body.Should().Contain("token=token-1");
		body.Should().Contain("token_type_hint=refresh_token");
	}

	[Fact]
	public async Task CreateUserAsync_ShouldReturnCreatedResourceIdentifier()
	{
		var handler = new StubHttpMessageHandler();
		handler.EnqueueJson("""{ "access_token": "admin-token" }""");
		handler.Enqueue(new HttpResponseMessage(HttpStatusCode.Created)
		{
			Headers =
			{
				Location = new Uri("https://auth.example.com/admin/realms/pivot/users/user-123")
			}
		});

		var service = new KeycloakIdentityProviderAdminService(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(Settings));

		var result = await service.CreateUserAsync(new CreateIdentityProviderUserRequest
		{
			Username = "pivot-user",
			Email = "user@example.com"
		});

		result.Should().Be("user-123");
	}

	[Fact]
	public async Task GetUserByEmailAsync_ShouldReturnMappedUser()
	{
		var handler = new StubHttpMessageHandler();
		handler.EnqueueJson("""{ "access_token": "admin-token" }""");
		handler.EnqueueJson("""
		[
		  {
		    "id": "user-1",
		    "username": "gihed",
		    "email": "gihed@example.com",
		    "firstName": "Gihed",
		    "lastName": "Annabi",
		    "enabled": true
		  }
		]
		""");

		var service = new KeycloakIdentityProviderAdminService(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(Settings));

		var result = await service.GetUserByEmailAsync("gihed@example.com");

		result.Should().NotBeNull();
		result!.Id.Should().Be("user-1");
		result.Email.Should().Be("gihed@example.com");
	}
	#endregion
}
