using Bar.CQRS.Commands.Tab;

namespace Bar.CQRS.Commands
{
    public class CloseTab : TabCommand
    {
        public decimal AmountPaid { get; set; }
    }
}
