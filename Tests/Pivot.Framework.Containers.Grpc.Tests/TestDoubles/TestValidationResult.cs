using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.Tests.TestDoubles;

/// <summary>
/// Test validation result used by gRPC status mapping tests.
/// </summary>
internal sealed class TestValidationResult : IValidationResult
{
	#region Properties
	/// <summary>
	/// Validation errors to expose to the mapper.
	/// </summary>
	public required IReadOnlyCollection<Error> Errors { get; init; }
	#endregion
}
