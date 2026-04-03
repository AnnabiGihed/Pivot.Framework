using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Pivot.Framework.Authentication.API.Contracts;
using Pivot.Framework.Authentication.Models;
using Pivot.Framework.Authentication.Services;
using Pivot.Framework.Authentication.Storage;

namespace Pivot.Framework.Authentication.API.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Minimal-API endpoint mapping helpers for backend authentication flows.
/// </summary>
public static class AuthenticationApiEndpointRouteBuilderExtensions
{
	/// <summary>
	/// Maps a standard authentication endpoint group.
	/// </summary>
	public static RouteGroupBuilder MapAuthenticationApi(this IEndpointRouteBuilder endpoints, string prefix = "/auth")
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var group = endpoints.MapGroup(prefix).WithTags("Authentication");
		group.MapPost("/login", AuthenticationApiHandlers.LoginAsync);
		group.MapPost("/callback", AuthenticationApiHandlers.CallbackAsync);
		group.MapPost("/refresh", AuthenticationApiHandlers.RefreshAsync);
		group.MapPost("/logout", AuthenticationApiHandlers.LogoutAsync);
		group.MapGet("/profile", AuthenticationApiHandlers.ProfileAsync);
		group.MapPost("/introspect", AuthenticationApiHandlers.IntrospectAsync);

		return group;
	}
}

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Endpoint handlers for the authentication API.
/// </summary>
public static class AuthenticationApiHandlers
{
	public static async Task<IResult> LoginAsync(
		[FromBody] AuthAuthorizationRequest request,
		[FromServices] IIdentityProviderAuthService authService,
		CancellationToken ct)
	{
		var result = await authService.BuildAuthorizationUrlAsync(request, ct);
		return Results.Ok(result);
	}

	public static async Task<IResult> CallbackAsync(
		[FromBody] AuthCodeExchangeRequest request,
		[FromServices] IIdentityProviderAuthService authService,
		[FromServices] IAuthSessionStore? sessionStore,
		CancellationToken ct)
	{
		var result = await authService.ExchangeAuthorizationCodeAsync(request, ct);
		await PersistSessionAsync(request.SessionId, result, sessionStore, ct);
		return Results.Ok(result);
	}

	public static async Task<IResult> RefreshAsync(
		[FromBody] AuthRefreshTokenRequest request,
		[FromServices] IIdentityProviderAuthService authService,
		[FromServices] IAuthSessionStore? sessionStore,
		CancellationToken ct)
	{
		var result = await authService.RefreshTokenAsync(request, ct);
		await PersistSessionAsync(request.SessionId, result, sessionStore, ct);
		return Results.Ok(result);
	}

	public static async Task<IResult> LogoutAsync(
		[FromBody] AuthLogoutRequest request,
		[FromServices] IIdentityProviderAuthService authService,
		[FromServices] IAuthSessionStore? sessionStore,
		CancellationToken ct)
	{
		await authService.LogoutAsync(request, ct);

		if (sessionStore is not null && !string.IsNullOrWhiteSpace(request.SessionId))
			await sessionStore.RemoveAsync(request.SessionId, ct);

		return Results.Ok();
	}

	public static async Task<IResult> ProfileAsync(
		HttpContext httpContext,
		[FromServices] IIdentityProviderAuthService authService,
		CancellationToken ct)
	{
		if (!TryGetBearerToken(httpContext, out var accessToken))
			return Results.Unauthorized();

		var profile = await authService.GetUserProfileAsync(accessToken!, ct);
		return Results.Ok(profile);
	}

	public static async Task<IResult> IntrospectAsync(
		[FromBody] IntrospectTokenRequest request,
		[FromServices] ITokenIntrospectionService introspectionService,
		CancellationToken ct)
	{
		var result = await introspectionService.IntrospectTokenAsync(request.Token, ct);
		return Results.Ok(result);
	}

	private static bool TryGetBearerToken(HttpContext httpContext, out string? accessToken)
	{
		accessToken = null;

		if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
			return false;

		var header = authorizationHeader.ToString();
		if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			return false;

		accessToken = header["Bearer ".Length..].Trim();
		return !string.IsNullOrWhiteSpace(accessToken);
	}

	private static async Task PersistSessionAsync(
		string? sessionId,
		AuthTokenResponse tokens,
		IAuthSessionStore? sessionStore,
		CancellationToken ct)
	{
		if (sessionStore is null || string.IsNullOrWhiteSpace(sessionId))
			return;

		var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);

		await sessionStore.SaveAsync(new AuthSession
		{
			SessionId = sessionId,
			SubjectId = jwtToken.Claims.FirstOrDefault(claim => claim.Type is "sub" or "nameid")?.Value,
			Username = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "preferred_username")?.Value,
			AccessToken = tokens.AccessToken,
			RefreshToken = tokens.RefreshToken,
			ExpiresAt = tokens.ExpiresAt
		}, ct);
	}
}
