using Bar.CQRS.Queries.Base;
using Bar.Domain.Views;
using System.Collections.Generic;

namespace Bar.CQRS.Queries.Bar
{
    public class GetInStockBeverages : IQuery<IEnumerable<BeverageView>>
    {
    }
}