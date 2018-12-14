using Bar.Domain.Events.Base;
using System;

namespace Bar.Domain.Events
{
    public class TabOpened : IEvent
    {
        public Guid TabId { get; set; }

        public string ClientName { get; set; }
    }
}
