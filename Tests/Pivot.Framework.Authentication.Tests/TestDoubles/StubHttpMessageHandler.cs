using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Pivot.Framework.Authentication.Tests.TestDoubles;

/// <summary>
/// Test double HTTP message handler that returns queued responses and records requests.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
	#region Dependencies
	private readonly ConcurrentQueue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
	#endregion

	#region Properties
	/// <summary>
	/// Captured requests in the order they were received.
	/// </summary>
	public List<HttpRequestMessage> Requests { get; } = [];
	#endregion

	#region Public Methods
	/// <summary>
	/// Enqueues a response to be returned for the next request.
	/// </summary>
	public void Enqueue(HttpResponseMessage response)
	{
		_responses.Enqueue(_ => response);
	}

	/// <summary>
	/// Enqueues a JSON response with the provided status code.
	/// </summary>
	public void EnqueueJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		Enqueue(new HttpResponseMessage(statusCode)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json")
		});
	}
	#endregion

	#region Overrides
	/// <summary>
	/// Returns the next queued response or throws when none are available.
	/// </summary>
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Requests.Add(await CloneRequestAsync(request, cancellationToken));

		if (_responses.TryDequeue(out var responseFactory))
			return responseFactory(request);

		throw new InvalidOperationException("No queued response configured for the stub HTTP handler.");
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Clones the incoming request so tests can inspect headers and content after disposal.
	/// </summary>
	private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original, CancellationToken cancellationToken)
	{
		var clone = new HttpRequestMessage(original.Method, original.RequestUri);

		foreach (var header in original.Headers)
			clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

		if (original.Content is not null)
		{
			var content = await original.Content.ReadAsStringAsync(cancellationToken);
			clone.Content = new StringContent(content, Encoding.UTF8, original.Content.Headers.ContentType?.MediaType);
		}

		return clone;
	}
	#endregion
}
