using MediatR;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Abstrations.Messaging.Commands;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
