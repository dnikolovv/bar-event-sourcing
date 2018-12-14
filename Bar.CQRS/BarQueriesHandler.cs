using Bar.CQRS.Queries.Bar;
using Bar.CQRS.Queries.Base;
using Bar.Data;
using Bar.Domain;
using Bar.Domain.Errors;
using Bar.Domain.Views;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bar.CQRS
{
    public class BarQueriesHandler :
        IQueryHandler<GetInStockBeverages, IEnumerable<BeverageView>>
    {
        private readonly ApplicationDbContext _dbContext;

        public BarQueriesHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Option<IEnumerable<BeverageView>, Error>> Handle(GetInStockBeverages request, CancellationToken cancellationToken) =>
            request
                .SomeNotNull<GetInStockBeverages, Error>(Errors.Generic.NullQuery)
                .MapAsync(async _ =>
                {
                    var beverages = await _dbContext
                        .Beverages
                        .ToListAsync(cancellationToken);

                    return beverages
                        .Select(b => new BeverageView
                        {
                            MenuNumber = b.MenuNumber,
                            Description = b.Description,
                            Price = b.Price
                        });
                });
    }
}