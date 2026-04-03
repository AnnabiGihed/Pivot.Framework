using System.Security.Claims;
using Grpc.Core;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

internal sealed class TestServerCallContext : ServerCallContext
{
	private readonly Metadata _requestHeaders = [];
	private readonly Metadata _responseTrailers = [];
	private readonly AuthContext _authContext = new(string.Empty, []);
	private Status _status;
	private WriteOptions? _writeOptions;

	public TestServerCallContext(CancellationToken cancellationToken = default)
	{
		CallCancellationToken = cancellationToken;
		DeadlineValue = DateTime.UtcNow.AddMinutes(1);
		MethodName = "/pivot.test/TestService/Execute";
		HostName = "localhost";
		PeerName = "ipv4:127.0.0.1:5001";
	}

	public CancellationToken CallCancellationToken { get; }

	public DateTime DeadlineValue { get; set; }

	public string HostName { get; set; }

	public string MethodName { get; set; }

	public string PeerName { get; set; }

	public ClaimsPrincipal User { get; set; } = new();

	protected override string MethodCore => MethodName;

	protected override string HostCore => HostName;

	protected override string PeerCore => PeerName;

	protected override DateTime DeadlineCore => DeadlineValue;

	protected override Metadata RequestHeadersCore => _requestHeaders;

	protected override CancellationToken CancellationTokenCore => CallCancellationToken;

	protected override Metadata ResponseTrailersCore => _responseTrailers;

	protected override Status StatusCore
	{
		get => _status;
		set => _status = value;
	}

	protected override WriteOptions? WriteOptionsCore
	{
		get => _writeOptions;
		set => _writeOptions = value;
	}

	protected override AuthContext AuthContextCore => _authContext;

	protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
		=> throw new NotSupportedException();

	protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
		=> Task.CompletedTask;

	protected override IDictionary<object, object> UserStateCore { get; } = new Dictionary<object, object>();
}
