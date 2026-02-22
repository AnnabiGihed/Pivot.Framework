using Templates.Core.Caching.Redis;
using Templates.Core.Caching.Handlers;
using Microsoft.Extensions.Configuration;
using Templates.Core.Caching.Abstractions;
using Templates.Core.Authentication.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Templates.Core.Caching.Extensions;

/// <summary>
/// Extension methods to add Redis-backed token caching and revocation to a backend API.
/// </summary>
public static class RedisCachingExtensions
{
	/// <summary>
	/// Registers the full Redis caching stack:
	/// - <see cref="ICacheService"/>              (generic typed cache)
	/// - <see cref="IDistributedTokenCache"/>     (JWT claims cache)
	/// - <see cref="ITokenRevocationCache"/>      (logout blacklist)
	///
	/// Reads the connection string from <c>ConnectionStrings:Redis</c>
	/// (or provide it via <paramref name="connectionString"/>).
	///
	/// <code>
	///   // appsettings.json
	///   "ConnectionStrings": { "Redis": "localhost:6379" }
	///
	///   // Program.cs
	///   builder.Services.AddRedisCache(builder.Configuration);
	/// </code>
	/// </summary>
	public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration, string? connectionString = null, string instanceName = "TemplatesCore:")
	{
		var connStr = connectionString ?? configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Missing Redis connection string. Add 'ConnectionStrings:Redis' to appsettings.json.");

		services.AddStackExchangeRedisCache(opt =>
		{
			opt.Configuration = connStr;
			opt.InstanceName = instanceName;
		});

		services.AddSingleton<ICacheService, RedisCacheService>();
		services.AddSingleton<IDistributedTokenCache, RedisDistributedTokenCache>();
		services.AddSingleton<ITokenRevocationCache, RedisTokenRevocationCache>();

		return services;
	}

	/// <summary>
	/// Composite one-liner that registers:
	/// 1. Keycloak JWT authentication (via <see cref="KeycloakServiceCollectionExtensions.AddKeycloakBackend"/>)
	/// 2. Redis caching (via <see cref="AddRedisCache"/>)
	/// 3. <see cref="KeycloakRedisJwtEvents"/> wired into the JWT bearer pipeline
	///
	/// <code>
	///   // Program.cs — replaces AddKeycloakBackend + AddRedisCache:
	///   builder.Services.AddKeycloakRedisCache(builder.Configuration);
	/// </code>
	///
	/// appsettings.json must contain both "Keycloak" and "ConnectionStrings:Redis":
	/// <code>
	///   {
	///     "Keycloak": { "BaseUrl": "...", "Realm": "...", "ClientId": "...", "Audience": "..." },
	///     "ConnectionStrings": { "Redis": "localhost:6379" }
	///   }
	/// </code>
	/// </summary>
	public static IServiceCollection AddKeycloakRedisCache(this IServiceCollection services, IConfiguration configuration, string? redisConnectionString = null, string redisInstanceName = "TemplatesCore:")
	{
		// 1. Redis services
		services.AddRedisCache(configuration, redisConnectionString, redisInstanceName);

		// 2. Redis JWT events (must be before AddKeycloakBackend so we can inject)
		services.AddSingleton<KeycloakRedisJwtEvents>();

		// 3. Keycloak authentication — pass our Redis events into the JWT bearer pipeline
		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUser, CurrentUser>();

		services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
			{
				var keycloak = configuration
					.GetSection(KeycloakOptions.SectionName)
					.Get<KeycloakOptions>()!;

				keycloak.Validate();

				jwt.Authority = keycloak.IssuerUrl;
				jwt.Audience = keycloak.Audience;
				jwt.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
				jwt.MetadataAddress = keycloak.MetadataUrl;

				jwt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = keycloak.IssuerUrl,
					ValidateAudience = true,
					ValidAudience = keycloak.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromSeconds(30),
					RoleClaimType = "roles",
					NameClaimType = "preferred_username",
				};

				// Resolve our Redis events from DI — this is the key integration point
				jwt.EventsType = typeof(KeycloakRedisJwtEvents);
			});

		services.AddAuthorization();

		return services;
	}

	/// <summary>
	/// Revokes a specific access token and removes it from the claims cache.
	/// Call this in your logout endpoint.
	///
	/// <code>
	///   app.MapPost("/logout", async (
	///       ITokenRevocationCache revocation,
	///       IDistributedTokenCache tokenCache,
	///       HttpContext ctx) =>
	///   {
	///       var token = ctx.GetBearerToken();
	///       await revocation.RevokeAsync(token, DateTimeOffset.UtcNow.AddHours(1));
	///       await tokenCache.InvalidateAsync(token);
	///       return Results.NoContent();
	///   });
	/// </code>
	/// </summary>
	public static string? GetBearerToken(this Microsoft.AspNetCore.Http.HttpContext ctx)
	{
		return ctx.Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim().NullIfEmpty();
	}

	private static string? NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
}