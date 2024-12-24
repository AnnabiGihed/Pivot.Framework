using MediatR;
using Templates.Core.Domain.Shared;

namespace Templates.Core.Application.Abstrations.Messaging.Queries;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>
{
}
