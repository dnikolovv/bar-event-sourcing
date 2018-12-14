using Bar.Domain.Events.Base;
using System;

namespace Bar.Domain.Events
{
    public class TabClosed : IEvent
    {
        public Guid TabId { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TipValue { get; set; }
        public decimal OrderValue { get; set; }
    }
}
