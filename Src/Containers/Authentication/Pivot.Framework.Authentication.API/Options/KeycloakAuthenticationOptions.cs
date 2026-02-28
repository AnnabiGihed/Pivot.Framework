using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pivot.Framework.Authentication.AspNetCore.Options;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Fluent options builder for <see cref="KeycloakAuthenticationExtensions.AddKeycloakAuthentication"/>.
///              Controls which optional features are activated on top of the core JWT bearer setup.
///              Additional packages (e.g. Pivot.Framework.Authentication.Caching) extend this
///              class via extension methods to add their own registrations without creating
///              a hard compile-time dependency between packages.
/// </summary>
public sealed class KeycloakAuthenticationOptions
{
	#region Internal State
	internal bool UseCurrentUser { get; private set; }
	internal bool UseSwagger { get; private set; }
	internal string? SwaggerTitle { get; private set; }
	internal string? SwaggerVersion { get; private set; }

	/// <summary>
	/// Callbacks registered by extension packages (e.g. caching).
	/// Each callback receives the DI container and the application configuration.
	/// </summary>
	internal List<Action<IServiceCollection, IConfiguration>> AdditionalRegistrations { get; } = [];
	#endregion

	#region Built-in Options
	/// <summary>
	/// Registers <see cref="ICurrentUser"/> (scoped) and <see cref="IHttpContextAccessor"/>.
	/// Required for any API that needs to resolve the current authenticated user inside
	/// application or domain services.
	/// </summary>
	public KeycloakAuthenticationOptions WithCurrentUser()
	{
		UseCurrentUser = true;
		return this;
	}

	/// <summary>
	/// Registers Swagger / Swashbuckle with the Keycloak OAuth2 Authorization Code + PKCE
	/// security definition and a global security requirement so every endpoint shows the lock icon.
	/// </summary>
	/// <param name="title">The Swagger document title displayed in the UI (e.g. "My API").</param>
	/// <param name="version">The Swagger document version (e.g. "v1").</param>
	public KeycloakAuthenticationOptions WithSwagger(string title, string version = "v1")
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(title);
		ArgumentException.ThrowIfNullOrWhiteSpace(version);

		UseSwagger = true;
		SwaggerTitle = title;
		SwaggerVersion = version;

		return this;
	}
	#endregion

	#region Extensibility Hook
	/// <summary>
	/// Called by external packages (e.g. <c>Pivot.Framework.Authentication.Caching</c>) to inject
	/// additional DI registrations into the pipeline without creating a hard package dependency.
	/// </summary>
	public void AddRegistration(Action<IServiceCollection, IConfiguration> registration)
	{
		ArgumentNullException.ThrowIfNull(registration);
		AdditionalRegistrations.Add(registration);
	}
	#endregion
}