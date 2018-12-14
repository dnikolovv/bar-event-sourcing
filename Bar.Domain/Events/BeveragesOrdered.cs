using Bar.Domain.Events.Base;
using System;
using System.Collections.Generic;

namespace Bar.Domain.Events
{
    public class BeveragesOrdered : IEvent
    {
        public Guid TabId { get; set; }

        public ICollection<Beverage> Beverages { get; set; }
    }
}
