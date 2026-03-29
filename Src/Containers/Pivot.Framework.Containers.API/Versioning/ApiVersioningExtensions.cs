using Microsoft.Extensions.DependencyInjection;
using Asp.Versioning;

namespace Pivot.Framework.Containers.API.Versioning;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : DI extension methods for configuring API versioning on ASP.NET Core services.
///              Supports URL segment, header, and query string versioning strategies.
/// </summary>
public static class ApiVersioningExtensions
{
	/// <summary>
	/// Adds API versioning with default configuration.
	/// Defaults to URL segment versioning (e.g., /api/v1/resources).
	/// </summary>
	public static IServiceCollection AddPivotApiVersioning(
		this IServiceCollection services,
		int defaultMajorVersion = 1,
		int defaultMinorVersion = 0)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(defaultMajorVersion, defaultMinorVersion);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;
			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader(),
				new HeaderApiVersionReader("X-Api-Version"),
				new QueryStringApiVersionReader("api-version"));
		})
		.AddApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'VVV";
			options.SubstituteApiVersionInUrl = true;
		});

		return services;
	}
}
