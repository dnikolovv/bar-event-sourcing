using Bar.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bar.Domain.Views
{
    public class TabView
    {
        public Guid Id { get; set; }

        public string ClientName { get; set; }

        public List<Beverage> OrderedBeverages { get; set; } = new List<Beverage>();

        public List<Beverage> ServedBeverages { get; set; } = new List<Beverage>();

        public List<Beverage> OutstandingBeverages { get; set; } = new List<Beverage>();

        public decimal ServedItemsValue { get; set; }

        public bool IsOpen { get; set; }

        public decimal TipValue { get; set; }

        public decimal TotalPaid { get; set; }

        public void ApplyEvent(TabOpened @event)
        {
            IsOpen = true;
            ClientName = @event.ClientName;
        }

        public void ApplyEvent(TabClosed @event)
        {
            IsOpen = false;
            TotalPaid += @event.AmountPaid;
            TipValue += @event.TipValue;
            ServedItemsValue = @event.OrderValue;
        }

        public void ApplyEvent(BeveragesOrdered @event)
        {
            OrderedBeverages.AddRange(@event.Beverages);
            OutstandingBeverages.AddRange(@event.Beverages);
        }

        public void ApplyEvent(BeveragesServed @event)
        {
            ServedItemsValue += @event.Beverages.Sum(b => b.Price);
            ServedBeverages.AddRange(@event.Beverages);

            foreach (var servedBeverage in @event.Beverages)
            {
                var outstanding = OutstandingBeverages
                    .FirstOrDefault(b => b.MenuNumber == servedBeverage.MenuNumber);

                if (outstanding != null)
                {
                    OutstandingBeverages.Remove(outstanding);
                }
            }
        }
    }
}
