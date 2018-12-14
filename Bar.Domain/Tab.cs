using Bar.Domain.Base;
using Bar.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bar.Domain
{
    public class Tab : IAggregate
    {
        public Tab()
        {
        }

        public Tab(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public string ClientName { get; set; }
        public bool IsOpen { get; set; }
        public List<Beverage> OutstandingBeverages { get; set;  } = new List<Beverage>();
        public decimal ServedItemsValue { get; set; }

        public void Apply(TabOpened @event)
        {
            IsOpen = true;
            ClientName = @event.ClientName;
        }

        public void Apply(TabClosed @event)
        {
            IsOpen = false;
        }

        public void Apply(BeveragesOrdered @event) =>
            OutstandingBeverages.AddRange(@event.Beverages);

        public void Apply(BeveragesServed @event)
        {
            ServedItemsValue += @event.Beverages.Sum(b => b.Price);
            
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

        public BeveragesServed ServeBeverages(List<Beverage> beverages) =>
            new BeveragesServed
            {
                TabId = Id,
                Beverages = beverages
            };

        public BeveragesOrdered OrderBeverages(List<Beverage> beverages) =>
            new BeveragesOrdered
            {
                TabId = Id,
                Beverages = beverages
            };

        public TabClosed CloseTab(decimal amountPaid) =>
            new TabClosed
            {
                TabId = Id,
                TipValue = amountPaid - ServedItemsValue,
                OrderValue = ServedItemsValue,
                AmountPaid = amountPaid
            };

        public TabOpened OpenTab(string clientName) =>
            new TabOpened
            {
                TabId = Id,
                ClientName = clientName
            };
    }
}