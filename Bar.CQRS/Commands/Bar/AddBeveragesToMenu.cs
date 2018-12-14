using Bar.CQRS.Commands.Base;
using Bar.Domain.Views;
using System.Collections.Generic;

namespace Bar.CQRS.Commands.Bar
{
    public class AddBeveragesToMenu : ICommand
    {
        public ICollection<BeverageView> Beverages { get; set; } = new List<BeverageView>();
    }
}
