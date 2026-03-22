namespace Pivot.Framework.Authentication.Hangfire.Constants;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 02-2026
/// Purpose     : Scheme name constants shared across the Hangfire Keycloak auth helpers.
///              Kept internal — consumers never need to reference scheme names directly.
/// </summary>
internal static class HangfireAuthConstants
{
	#region Constants
	/// <summary>
	/// The cookie authentication scheme name used for the Hangfire dashboard browser flow.
	/// </summary>
	internal const string CookieScheme = "HangfireCookie";

	/// <summary>
	/// The OpenID Connect authentication scheme name used for the Hangfire dashboard Keycloak redirect.
	/// </summary>
	internal const string OidcScheme = "HangfireOidc";
	#endregion
}