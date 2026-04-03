using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

internal sealed class TestValidationResult : IValidationResult
{
	public required IReadOnlyCollection<Error> Errors { get; init; }
}
