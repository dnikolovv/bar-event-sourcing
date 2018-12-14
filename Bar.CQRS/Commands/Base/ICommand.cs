using Bar.Domain;
using MediatR;
using Optional;

namespace Bar.CQRS.Commands.Base
{
    public interface ICommand : IRequest<Option<Unit, Error>>
    {
    }
}
