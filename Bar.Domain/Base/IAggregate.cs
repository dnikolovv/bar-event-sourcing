using System;

namespace Bar.Domain.Base
{
    public interface IAggregate
    {
        Guid Id { get; set; }
    }
}
