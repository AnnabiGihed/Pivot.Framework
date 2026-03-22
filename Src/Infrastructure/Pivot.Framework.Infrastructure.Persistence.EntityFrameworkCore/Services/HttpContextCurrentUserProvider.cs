using Microsoft.AspNetCore.Http;
using Pivot.Framework.Application.Abstractions;

namespace Pivot.Framework.Infrastructure.Persistence.EntityFrameworkCore.Services;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 01-2026
/// Purpose     : Resolves the current user identity from the HTTP context.
///              Returns the authenticated user's name, or "System" as a fallback
///              when no HTTP context or authenticated identity is available.
/// </summary>
public class HttpContextCurrentUserProvider : ICurrentUserProvider
{
	#region Fields
	/// <summary>
	/// The HTTP context accessor used to retrieve the current request context.
	/// </summary>
	private readonly IHttpContextAccessor _httpContextAccessor;
	#endregion

	#region Constructors
	/// <summary>
	/// Initialises a new <see cref="HttpContextCurrentUserProvider"/> with the provided
	/// <see cref="IHttpContextAccessor"/>.
	/// </summary>
	/// <param name="httpContextAccessor">The HTTP context accessor. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContextAccessor"/> is null.</exception>
	public HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Returns the name of the currently authenticated user from the HTTP context.
	/// Falls back to "System" when no authenticated identity is available.
	/// </summary>
	/// <returns>The current user's name, or "System" if unavailable.</returns>
	public string GetCurrentUser()
	{
		return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
	}
	#endregion
}
