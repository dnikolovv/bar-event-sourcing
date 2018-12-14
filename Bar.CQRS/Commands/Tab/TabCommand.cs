using Bar.CQRS.Commands.Base;
using System;

namespace Bar.CQRS.Commands.Tab
{
    public abstract class TabCommand : ICommand
    {
        public Guid TabId { get; set; }
    }
}
