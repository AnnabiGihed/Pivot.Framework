using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Pivot.Framework.Authentication.Tests.TestDoubles;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
	private readonly ConcurrentQueue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

	public List<HttpRequestMessage> Requests { get; } = [];

	public void Enqueue(HttpResponseMessage response)
	{
		_responses.Enqueue(_ => response);
	}

	public void EnqueueJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		Enqueue(new HttpResponseMessage(statusCode)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json")
		});
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Requests.Add(await CloneRequestAsync(request, cancellationToken));

		if (_responses.TryDequeue(out var responseFactory))
			return responseFactory(request);

		throw new InvalidOperationException("No queued response configured for the stub HTTP handler.");
	}

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
}
