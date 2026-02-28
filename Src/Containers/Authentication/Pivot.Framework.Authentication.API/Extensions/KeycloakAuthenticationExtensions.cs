using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Pivot.Framework.Authentication.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Pivot.Framework.Authentication.AspNetCore.Helpers;
using Pivot.Framework.Authentication.AspNetCore.Options;

namespace Pivot.Framework.Authentication.AspNetCore.Extensions;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Single public entry point for all Keycloak authentication registrations.
///              Use <see cref="AddKeycloakAuthentication"/> with a <see cref="KeycloakAuthenticationOptions"/>
///              builder to opt into features — JWT bearer is always registered; everything else
///              (ICurrentUser, Redis token caching, Swagger) is activated explicitly.
///
///              The internal helpers below are consumed by the options callbacks registered by
///              sibling packages (e.g. Pivot.Framework.Authentication.Caching) and must not be
///              called directly from application code.
/// </summary>
public static class KeycloakAuthenticationExtensions
{
	#region Public API — single entry point

	/// <summary>
	/// Registers Keycloak JWT bearer authentication.
	/// </summary>
	/// <example>
	/// Minimal (JWT only):
	/// <code>services.AddKeycloakAuthentication(configuration);</code>
	///
	/// Standard API:
	/// <code>
	/// services.AddKeycloakAuthentication(configuration, o => o
	///     .WithCurrentUser());
	/// </code>
	///
	/// Full production stack (Redis token caching + Swagger):
	/// <code>
	/// services.AddKeycloakAuthentication(configuration, o => o
	///     .WithCurrentUser()
	///     .WithRedisTokenCaching()   // requires Pivot.Framework.Authentication.Caching
	///     .WithSwagger("My API", "v1"));
	/// </code>
	/// </example>
	public static IServiceCollection AddKeycloakAuthentication(
		this IServiceCollection services,
		IConfiguration configuration,
		Action<KeycloakAuthenticationOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		var options = new KeycloakAuthenticationOptions();
		configure?.Invoke(options);

		// Always: core JWT bearer
		services.RegisterCoreJwtBearer(configuration);

		// Optional: ICurrentUser + IHttpContextAccessor
		if (options.UseCurrentUser)
			services.RegisterCurrentUser();

		// Optional: Swagger OAuth2 PKCE
		if (options.UseSwagger)
			services.RegisterSwagger(configuration, options.SwaggerTitle!, options.SwaggerVersion!);

		// Optional: any registrations injected by external packages (e.g. Redis caching)
		foreach (var registration in options.AdditionalRegistrations)
			registration(services, configuration);

		return services;
	}

	#endregion

	#region Internal Helpers

	/// <summary>
	/// Core JWT bearer setup. Always called. Not intended for direct use.
	/// </summary>
	internal static IServiceCollection RegisterCoreJwtBearer(
		this IServiceCollection services,
		IConfiguration configuration,
		Action<JwtBearerOptions>? configureJwt = null)
	{
		var keycloakOptions = configuration
			.GetSection(KeycloakOptions.SectionName)
			.Get<KeycloakOptions>()
			?? throw new InvalidOperationException($"Missing configuration section '{KeycloakOptions.SectionName}'.");

		keycloakOptions.Validate();

		services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
			{
				jwt.Authority = keycloakOptions.IssuerUrl;
				jwt.Audience = keycloakOptions.Audience;
				jwt.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
				jwt.MetadataAddress = keycloakOptions.MetadataUrl;

				jwt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = keycloakOptions.IssuerUrl,
					ValidateAudience = true,
					ValidAudience = keycloakOptions.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromSeconds(30),
					RoleClaimType = ClaimTypes.Role,
					NameClaimType = "preferred_username"
				};

				jwt.Events = new JwtBearerEvents
				{
					OnTokenValidated = ctx =>
					{
						KeycloakClaimsTransformer.FlattenRoles(ctx);
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = ctx =>
					{
						var logger = ctx.HttpContext.RequestServices
							.GetRequiredService<ILogger<JwtBearerEvents>>();
						logger.LogWarning(ctx.Exception, "Keycloak authentication failed: {Message}", ctx.Exception.Message);
						return Task.CompletedTask;
					}
				};

				configureJwt?.Invoke(jwt);
			});

		services.AddAuthorization();

		return services;
	}

	/// <summary>
	/// Registers <see cref="ICurrentUser"/> and <see cref="IHttpContextAccessor"/>. Not intended for direct use.
	/// </summary>
	internal static IServiceCollection RegisterCurrentUser(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUser, CurrentUser>();
		return services;
	}

	/// <summary>
	/// Registers Swagger with Keycloak OAuth2 PKCE. Not intended for direct use.
	/// </summary>
	internal static IServiceCollection RegisterSwagger(
		this IServiceCollection services,
		IConfiguration configuration,
		string title,
		string version)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc(version, new() { Title = title, Version = version });
			c.AddKeycloakSecurityDefinition(configuration);
			c.AddKeycloakSecurityRequirement();
		});

		return services;
	}

	#endregion
}