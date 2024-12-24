using MediatR;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Abstrations.Messaging.Commands;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>
{
}