namespace Pivot.Framework.Authentication.Hangfire.Constants;

/// <summary>
/// Scheme name constants shared across the Hangfire Keycloak auth helpers.
/// Kept internal — consumers never need to reference scheme names directly.
/// </summary>
internal static class HangfireAuthConstants
{
	internal const string CookieScheme = "HangfireCookie";
	internal const string OidcScheme = "HangfireOidc";
}