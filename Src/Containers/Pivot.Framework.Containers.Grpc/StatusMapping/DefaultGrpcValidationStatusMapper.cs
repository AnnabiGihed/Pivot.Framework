using System.Text.Json;
using Grpc.Core;
using Pivot.Framework.Containers.Grpc.Abstractions;
using Pivot.Framework.Domain.Shared;

namespace Pivot.Framework.Containers.Grpc.StatusMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Serialises validation errors into gRPC trailers using a transport-safe JSON payload.
/// </summary>
public sealed class DefaultGrpcValidationStatusMapper : IGrpcValidationStatusMapper
{
	private const string ValidationErrorsKey = "validation-errors";

	#region Methods

	/// <inheritdoc />
	public Metadata CreateTrailers(IReadOnlyCollection<Error> errors)
	{
		ArgumentNullException.ThrowIfNull(errors);

		var trailers = new Metadata();
		if (errors.Count == 0)
			return trailers;

		var payload = errors.Select(error => new
		{
			error.Code,
			error.Message
		});

		trailers.Add(ValidationErrorsKey, JsonSerializer.Serialize(payload));
		return trailers;
	}

	#endregion
}
