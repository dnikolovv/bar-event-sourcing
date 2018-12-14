using System.Collections.Generic;

namespace Bar.CQRS.Commands.Tab
{
    public class ServeBeverages : TabCommand
    {
        public IEnumerable<int> MenuNumbers { get; set; }
    }
}
