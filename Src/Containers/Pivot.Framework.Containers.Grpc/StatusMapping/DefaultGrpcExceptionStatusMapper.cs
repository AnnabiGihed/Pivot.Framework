using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Pivot.Framework.Application.Exceptions;
using Pivot.Framework.Containers.Grpc.Abstractions;

namespace Pivot.Framework.Containers.Grpc.StatusMapping;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 04-2026
/// Purpose     : Maps application exceptions to gRPC transport status and trailers.
/// </summary>
public sealed class DefaultGrpcExceptionStatusMapper : IGrpcExceptionStatusMapper
{
	#region Fields

	private readonly IHostEnvironment _environment;
	private readonly IGrpcValidationStatusMapper _validationStatusMapper;

	#endregion

	#region Constructors

	/// <summary>
	/// Initialises a new mapper with the required environment and validation dependencies.
	/// </summary>
	/// <param name="environment">The host environment used to control detail exposure.</param>
	/// <param name="validationStatusMapper">The validation mapper used for structured trailers.</param>
	public DefaultGrpcExceptionStatusMapper(
		IHostEnvironment environment,
		IGrpcValidationStatusMapper validationStatusMapper)
	{
		_environment = environment ?? throw new ArgumentNullException(nameof(environment));
		_validationStatusMapper = validationStatusMapper ?? throw new ArgumentNullException(nameof(validationStatusMapper));
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public GrpcStatusMapping Map(Exception exception)
	{
		ArgumentNullException.ThrowIfNull(exception);

		return exception switch
		{
			ValidationException validationException => new GrpcStatusMapping(
				StatusCode.InvalidArgument,
				validationException.Message,
				_validationStatusMapper.CreateTrailers(validationException.ValidationErrors)),

			BadRequestException badRequestException => new GrpcStatusMapping(
				StatusCode.InvalidArgument,
				badRequestException.Message,
				_validationStatusMapper.CreateTrailers(badRequestException.ValidationErrors)),

			NotFoundException notFoundException => new GrpcStatusMapping(
				StatusCode.NotFound,
				notFoundException.Message),

			RpcException rpcException => new GrpcStatusMapping(
				rpcException.StatusCode,
				rpcException.Status.Detail,
				rpcException.Trailers),

			_ => new GrpcStatusMapping(
				StatusCode.Internal,
				_environment.IsProduction() ? "An unexpected error occurred." : exception.ToString())
		};
	}

	#endregion
}
