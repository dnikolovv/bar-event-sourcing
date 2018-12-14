using Bar.Domain;
using MediatR;
using Optional;

namespace Bar.CQRS.Queries.Base
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Option<TResponse, Error>>
           where TQuery : IQuery<TResponse>
    {
    }
}
