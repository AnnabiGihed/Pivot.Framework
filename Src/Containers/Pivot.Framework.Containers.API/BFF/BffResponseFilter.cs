using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pivot.Framework.Infrastructure.Abstraction.BFF;

namespace Pivot.Framework.Containers.API.BFF;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Action filter that converts <see cref="BffResponse{T}"/> into appropriate HTTP responses.
///              Returns 503 with Retry-After header when a write depends on an unavailable downstream service.
///              Returns 200 with degradation metadata for partial-failure reads.
///              Implements MDM spec invariant #32.
/// </summary>
public sealed class BffResponseFilter : IAsyncResultFilter
{
	/// <inheritdoc />
	public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
		{
			var valueType = objectResult.Value.GetType();
			if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(BffResponse<>))
			{
				var availabilityProp = valueType.GetProperty(nameof(BffResponse<object>.Availability));
				var retryAfterProp = valueType.GetProperty(nameof(BffResponse<object>.RetryAfterSeconds));

				if (availabilityProp?.GetValue(objectResult.Value) is DataAvailability availability)
				{
					if (availability == DataAvailability.Unavailable)
					{
						objectResult.StatusCode = 503;
						var retryAfter = retryAfterProp?.GetValue(objectResult.Value) as int?;
						if (retryAfter.HasValue)
						{
							context.HttpContext.Response.Headers["Retry-After"] = retryAfter.Value.ToString();
						}
					}
				}
			}
		}

		await next();
	}
}
