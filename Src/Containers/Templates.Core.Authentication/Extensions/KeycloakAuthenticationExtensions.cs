using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Templates.Core.Authentication.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Templates.Core.Authentication.Models;

namespace Templates.Core.Authentication.Extensions;

/// <summary>
/// Extension methods to register Keycloak JWT authentication on ASP.NET Core backend.
/// </summary>
public static class KeycloakAuthenticationExtensions
{
	/// <summary>
	/// Registers Keycloak JWT bearer authentication.
	/// 
	/// Usage in Program.cs:
	/// <code>
	///   builder.Services.AddKeycloakAuthentication(builder.Configuration);
	/// </code>
	///
	/// Reads config from appsettings.json:
	/// <code>
	///   "Keycloak": {
	///     "BaseUrl": "https://auth.example.com",
	///     "Realm": "my-realm",
	///     "ClientId": "my-api",
	///     "Audience": "my-api",
	///     "RequireHttpsMetadata": true
	///   }
	/// </code>
	/// </summary>
	public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration,
		Action<JwtBearerOptions>? configureOptions = null)
	{
		var options = configuration
			.GetSection(KeycloakOptions.SectionName)
			.Get<KeycloakOptions>()
			?? throw new InvalidOperationException(
				$"Missing configuration section '{KeycloakOptions.SectionName}'.");

		options.Validate();

		// Make options available for injection (e.g., by Swagger extensions)
		services.Configure<KeycloakOptions>(
			configuration.GetSection(KeycloakOptions.SectionName));

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
			{
				jwt.Authority = options.IssuerUrl;
				jwt.Audience = options.Audience;
				jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
				jwt.MetadataAddress = options.MetadataUrl;

				jwt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = options.IssuerUrl,

					ValidateAudience = true,
					ValidAudience = options.Audience,

					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromSeconds(30),

					// Keycloak puts roles in "realm_access.roles" and
					// "resource_access.<clientId>.roles" — map them to standard ClaimTypes
					RoleClaimType = "roles",
					NameClaimType = "preferred_username",
				};

				jwt.Events = new JwtBearerEvents
				{
					OnTokenValidated = ctx =>
					{
						// Flatten Keycloak realm roles into standard role claims
						KeycloakClaimsTransformer.FlattenRoles(ctx);
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = ctx =>
					{
						// Log authentication failures without exposing details to callers
						var logger = ctx.HttpContext.RequestServices
							.GetRequiredService<Microsoft.Extensions.Logging.ILogger<JwtBearerEvents>>();
						logger.LogWarning(ctx.Exception,
							"Keycloak authentication failed: {Message}", ctx.Exception.Message);
						return Task.CompletedTask;
					}
				};

				// Allow caller to override further
				configureOptions?.Invoke(jwt);
			});

		services.AddAuthorization();

		return services;
	}
}