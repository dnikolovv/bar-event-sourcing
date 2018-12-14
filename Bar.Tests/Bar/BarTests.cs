using AutoFixture.Xunit2;
using Bar.CQRS.Commands.Bar;
using Bar.CQRS.Queries.Bar;
using Bar.Domain.Errors;
using Bar.Domain.Views;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bar.Tests.Bar
{
    public class BarTests : ResetDatabaseLifetime
    {
        private readonly SliceFixture _fixture;

        public BarTests()
        {
            _fixture = new SliceFixture();
        }

        [Theory]
        [AutoData]
        public async Task CanGetAllInStockBeverages(List<BeverageView> beveragesToAdd)
        {
            // Arrange
            await _fixture.SendAsync(new AddBeveragesToMenu
            {
                Beverages = beveragesToAdd
            });

            // Act
            var result = await _fixture.SendAsync(new GetInStockBeverages());

            // Assert
            result.HasValue.ShouldBeTrue();

            result.Exists(r => r.All(b => beveragesToAdd.Any(bv =>
                b.MenuNumber == bv.MenuNumber &&
                b.Price == bv.Price &&
                b.Description == bv.Description)));
        }

        [Theory]
        [AutoData]
        public async Task CanAddBeveragesInMenu(List<BeverageView> beveragesToAdd)
        {
            // Arrange
            var command = new AddBeveragesToMenu
            {
                Beverages = beveragesToAdd
            };

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.HasValue.ShouldBeTrue();

            await _fixture.ExecuteDbContextAsync(async dbContext =>
            {
                var beveragesInDb = await dbContext.Beverages.ToListAsync();

                beveragesInDb.ShouldAllBe(b => beveragesToAdd.Any(bv =>
                    b.MenuNumber == bv.MenuNumber &&
                    b.Price == bv.Price &&
                    b.Description == bv.Description));
            });
        }

        [Theory]
        [AutoData]
        public async Task CannotAddExistingBeverages(List<BeverageView> beverages)
        {
            // Arrange
            var addBeveragesCommand = new AddBeveragesToMenu { Beverages = beverages };

            await _fixture.SendAsync(addBeveragesCommand);

            // Act
            var result = await _fixture.SendAsync(addBeveragesCommand);

            // Assert
            result.HasValue.ShouldBeFalse();

            var expectedError = Errors.Beverage.AlreadyExist(beverages.Select(b => b.MenuNumber).ToArray());

            result.MatchNone(error => error
                .Messages
                .ShouldContain(expectedError));
        }
    }
}
