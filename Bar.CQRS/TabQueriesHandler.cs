using Bar.CQRS.Queries.Base;
using Bar.CQRS.Queries.Tab;
using Bar.Domain;
using Bar.Domain.Views;
using Marten;
using Optional;
using System.Threading;
using System.Threading.Tasks;

namespace Bar.CQRS
{
    public class TabQueriesHandler : IQueryHandler<GetTabView, TabView>
    {
        private readonly IDocumentSession _session;

        public TabQueriesHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<Option<TabView, Error>> Handle(GetTabView request, CancellationToken cancellationToken) =>
            (await _session
                .Query<TabView>()
                .SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken))
            .SomeNotNull<TabView, Error>($"No tab with an id of {request.Id} was found.");
    }
}
