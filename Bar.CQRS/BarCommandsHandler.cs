using Bar.CQRS.Commands.Bar;
using Bar.CQRS.Commands.Base;
using Bar.Data;
using Bar.Domain;
using Bar.Domain.Errors;
using Bar.Domain.Views;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bar.CQRS
{
    public class BarCommandsHandler :
        ICommandHandler<AddBeveragesToMenu>
    {
        private readonly ApplicationDbContext _dbContext;

        public BarCommandsHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Option<Unit, Error>> Handle(AddBeveragesToMenu request, CancellationToken cancellationToken)
        {
            return ValidateRequest().FlatMapAsync(command =>
                   CheckIfBeveragesAreNotExisting(command.Beverages).FlatMapAsync(
                   PersistBeverages));

            Option<AddBeveragesToMenu, Error> ValidateRequest() =>
                request.SomeWhen<AddBeveragesToMenu, Error>(
                    r => r.Beverages?.Count > 0,
                    Errors.Beverage.MustAddAtLeastOneBeverage);
        }

        private async Task<Option<IEnumerable<Beverage>, Error>> CheckIfBeveragesAreNotExisting(ICollection<BeverageView> beverages)
        {
            var alreadyExisting = await _dbContext
                .Beverages
                .Where(b => beverages.Any(x => x.MenuNumber == b.MenuNumber))
                .ToListAsync();

            return alreadyExisting
                .SomeWhen<IEnumerable<Beverage>, Error>(x => !x.Any(), x => Errors.Beverage.AlreadyExist(x.Select(b => b.MenuNumber).ToArray()))
                .Map(_ => beverages
                .Select(b => new Beverage
                {
                    Description = b.Description,
                    MenuNumber = b.MenuNumber,
                    Price = b.Price
                }));
        }

        private async Task<Option<Unit, Error>> PersistBeverages(IEnumerable<Beverage> beverages)
        {
            await _dbContext.Beverages.AddRangeAsync(beverages);
            await _dbContext.SaveChangesAsync();

            return Unit.Value.Some<Unit, Error>();
        }
    }
}
