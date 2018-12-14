using Bar.CQRS.Queries.Base;
using Bar.Domain.Views;
using System;

namespace Bar.CQRS.Queries.Tab
{
    public class GetTabView : IQuery<TabView>
    {
        public Guid Id { get; set; }
    }
}
