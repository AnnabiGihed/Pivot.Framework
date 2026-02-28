using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Pivot.Framework.Authentication.Hangfire.Constants;
using Pivot.Framework.Authentication.Hangfire.Dashboard;

namespace Pivot.Framework.Authentication.Hangfire.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Extension methods that wire Keycloak browser-based authentication
///              into the Hangfire dashboard pipeline.
///
///              Two methods form a matched pair:
///
///              Service registration (DI):
///                services.AddHangfireKeycloakBrowserAuth(configuration);
///
///              Pipeline (middleware):
///                app.UseHangfireDashboardWithKeycloakAuth();
///
///              The Keycloak section in appsettings must contain BaseUrl, Realm,
///              ClientId, and optionally ClientSecret (for confidential clients)
///              and RequireHttpsMetadata. This is the same shape used by all other
///              Pivot.Framework.Authentication.* packages.
/// </summary>
public static class HangfireKeycloakAuthExtensions
{
	/// <summary>
	/// Registers a dedicated Cookie + Keycloak OIDC authentication scheme for the
	/// Hangfire dashboard browser flow.
	///
	/// Call this during service registration alongside your existing
	/// AddKeycloakAuthentication() call. The two schemes are independent —
	/// the Cookie/OIDC pair is used exclusively for /hangfire, while JWT Bearer
	/// remains the scheme for all API endpoints.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configuration">Application configuration (reads the Keycloak section).</param>
	/// <returns>The updated service collection.</returns>
	public static IServiceCollection AddHangfireKeycloakBrowserAuth(this IServiceCollection services, IConfiguration configuration)
	{
		var keycloakSection = configuration.GetSection(KeycloakOptions.SectionName);

		var realm = keycloakSection["Realm"] ?? throw new InvalidOperationException($"{KeycloakOptions.SectionName}:Realm is required.");
		var baseUrl = keycloakSection["BaseUrl"] ?? throw new InvalidOperationException($"{KeycloakOptions.SectionName}:BaseUrl is required.");
		var clientId = keycloakSection["ClientId"] ?? throw new InvalidOperationException($"{KeycloakOptions.SectionName}:ClientId is required.");

		// Optional — only required for confidential Keycloak clients.
		var clientSecret = keycloakSection["ClientSecret"];

		var requireHttps = bool.TryParse(keycloakSection["RequireHttpsMetadata"], out var rh) && rh;

		services
			.AddAuthentication()
			.AddCookie(HangfireAuthConstants.CookieScheme, o =>
			{
				o.LoginPath = "/hangfire-login";
				o.AccessDeniedPath = "/hangfire-login";
			})
			.AddOpenIdConnect(HangfireAuthConstants.OidcScheme, o =>
			{
				o.SignInScheme = HangfireAuthConstants.CookieScheme;
				o.Authority = $"{baseUrl}/realms/{realm}";
				o.ClientId = clientId;
				o.ClientSecret = clientSecret;
				o.ResponseType = "code";
				o.SaveTokens = true;
				o.RequireHttpsMetadata = requireHttps;
				o.CallbackPath = "/hangfire-callback";

				// Align claim types with the rest of the framework.
				o.TokenValidationParameters.NameClaimType = "preferred_username";
			});

		return services;
	}

	/// <summary>
	/// Mounts the Hangfire dashboard at /hangfire and registers the companion
	/// /hangfire-login and /hangfire-logout endpoints.
	///
	/// Unauthenticated browser requests to /hangfire are automatically redirected
	/// to Keycloak. After login, the browser lands back on /hangfire with a valid
	/// HangfireCookie session.
	///
	/// Requires AddHangfireKeycloakBrowserAuth() to have been called first.
	/// </summary>
	/// <param name="app">The web application.</param>
	/// <param name="configureDashboard">Optional action to further customise DashboardOptions (e.g. title, dark mode).</param>
	public static void UseHangfireDashboardWithKeycloakAuth(this WebApplication app, Action<DashboardOptions>? configureDashboard = null)
	{
		// Triggers the OIDC redirect to Keycloak login.
		app.MapGet("/hangfire-login", (HttpContext ctx) =>
			ctx.ChallengeAsync(
				HangfireAuthConstants.OidcScheme,
				new AuthenticationProperties { RedirectUri = "/hangfire" }));

		// Clears both the local cookie and the Keycloak SSO session.
		app.MapGet("/hangfire-logout", async (HttpContext ctx) =>
		{
			await ctx.SignOutAsync(HangfireAuthConstants.CookieScheme);
			await ctx.SignOutAsync(HangfireAuthConstants.OidcScheme);
		});

		var dashboardOptions = new DashboardOptions
		{
			Authorization = new IDashboardAuthorizationFilter[]
			{
				new HangfireCookieDashboardAuthorizationFilter()
			},
			DarkModeEnabled = false
		};

		configureDashboard?.Invoke(dashboardOptions);

		app.UseHangfireDashboard(options: dashboardOptions);
	}
}