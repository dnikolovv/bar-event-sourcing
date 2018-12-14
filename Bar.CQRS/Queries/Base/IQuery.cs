using Bar.Domain;
using MediatR;
using Optional;

namespace Bar.CQRS.Queries.Base
{
    public interface IQuery<TResponse> : IRequest<Option<TResponse, Error>>
    {
    }
}
