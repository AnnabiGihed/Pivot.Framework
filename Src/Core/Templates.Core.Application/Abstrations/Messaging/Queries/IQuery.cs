using MediatR;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Abstrations.Messaging.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
