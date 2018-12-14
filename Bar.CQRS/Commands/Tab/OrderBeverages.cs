using System.Collections.Generic;

namespace Bar.CQRS.Commands.Tab
{
    public class OrderBeverages : TabCommand
    {
        public IEnumerable<int> MenuNumbers { get; set; }
    }
}