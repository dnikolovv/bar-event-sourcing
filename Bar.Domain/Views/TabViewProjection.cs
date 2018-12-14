using Bar.Domain.Events;
using Marten.Events.Projections;
using System;

namespace Bar.Domain.Views
{
    public class TabViewProjection : ViewProjection<TabView, Guid>
    {
        public TabViewProjection()
        {
            ProjectEvent<TabOpened>((ev) => ev.TabId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<TabClosed>((ev) => ev.TabId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<BeveragesOrdered>((ev) => ev.TabId, (view, @event) => view.ApplyEvent(@event));
            ProjectEvent<BeveragesServed>((ev) => ev.TabId, (view, @event) => view.ApplyEvent(@event));
        }
    }
}
