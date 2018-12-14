using Bar.Domain;
using MediatR;
using Optional;

namespace Bar.CQRS.Commands.Base
{
    public interface ICommandHandler<in TCommand> :
        IRequestHandler<TCommand, Option<Unit, Error>>
        where TCommand : IRequest<Option<Unit, Error>>
    {
    }
}
