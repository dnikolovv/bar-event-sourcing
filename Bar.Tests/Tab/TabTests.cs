using AutoFixture.Xunit2;
using Bar.CQRS.Commands;
using Bar.CQRS.Commands.Tab;
using Bar.CQRS.Queries.Tab;
using Bar.Domain;
using Bar.Domain.Errors;
using Bar.Domain.Views;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bar.Tests.Tab
{
    public class TabTests : ResetDatabaseLifetime
    {
        private readonly SliceFixture _fixture;

        public TabTests()
        {
            _fixture = new SliceFixture();
        }

        [Theory]
        [AutoData]
        public async Task CanCloseTab(Guid tabId)
        {
            // Arrange
            await OpenTab(tabId);

            var closeTabCommand = new CloseTab
            {
                TabId = tabId,
                AmountPaid = 10.0m
            };

            // Act
            var result = await _fixture.SendAsync(closeTabCommand);

            // Assert
            result.HasValue.ShouldBeTrue();

            await AssertTabExists(
                tabId,
                t => t.Id == tabId &&
                t.IsOpen == false &&
                t.TotalPaid == closeTabCommand.AmountPaid &&
                // We haven't ordered anything, therefore we expect the full price to be counted as a tip
                t.TipValue == closeTabCommand.AmountPaid);
        }

        [Theory]
        [AutoData]
        public async Task CanOpenTab(Guid tabId, string clientName)
        {
            // Arrange
            var command = new OpenTab
            {
                TabId = tabId,
                ClientName = clientName
            };

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.HasValue.ShouldBeTrue();

            await AssertTabExists(
                tabId,
                t => t.Id == tabId &&
                t.ClientName == clientName &&
                t.IsOpen == true);
        }

        [Theory]
        [AutoData]
        public async Task CanServeOneBeverage(Guid tabId, List<Beverage> beverages)
        {
            // Arrange
            await OpenTab(tabId);
            await AddBeveragesToDb(beverages);

            var beverageToServe = beverages.First().MenuNumber;

            // We intentionally order it twice in order to be able to assert that only one was served
            await OrderBeverages(tabId, beverageToServe);
            await OrderBeverages(tabId, beverageToServe);

            // Act
            var result = await _fixture.SendAsync(new ServeBeverages
            {
                TabId = tabId,
                MenuNumbers = new[] { beverageToServe }
            });

            // Assert
            result.HasValue.ShouldBeTrue();

            await AssertTabExists(
                tabId,
                tab => tab.ServedBeverages.Count == 1 && // One served
                       tab.ServedBeverages.First().MenuNumber == beverageToServe &&
                       tab.OutstandingBeverages.Count == 1 && // And one left, because we oredered it twice
                       tab.OutstandingBeverages.First().MenuNumber == beverageToServe);
        }

        [Theory]
        [AutoData]
        public async Task CanServeOneBeverageTwiceInARow(Guid tabId, List<Beverage> beverages)
        {
            // Arrange
            await OpenTab(tabId);
            await AddBeveragesToDb(beverages);

            var beverageNumberToServe = beverages.First().MenuNumber;

            // We intentionally order it twice in order to be able to assert that only one was served
            await OrderBeverages(tabId, beverageNumberToServe);
            await OrderBeverages(tabId, beverageNumberToServe);

            // Act
            var command = new ServeBeverages
            {
                TabId = tabId,
                MenuNumbers = new[] { beverageNumberToServe }
            };

            await _fixture.SendAsync(command);
            await _fixture.SendAsync(command);

            // Assert
            await AssertTabExists(
                tabId,
                tab => tab.ServedBeverages.Count == 2 &&
                       tab.ServedBeverages.All(b => b.MenuNumber == beverageNumberToServe));
        }

        [Theory]
        [AutoData]
        public async Task CanServeAllBeverages(Guid tabId, List<Beverage> beverages)
        {
            // Arrange
            await OpenTab(tabId);
            await AddBeveragesToDb(beverages);

            var beverageMenuNumbers = beverages.Select(b => b.MenuNumber).ToArray();

            await OrderBeverages(tabId, beverageMenuNumbers);

            // Act
            var result = await _fixture.SendAsync(new ServeBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageMenuNumbers
            });

            // Assert
            result.HasValue.ShouldBeTrue();

            await AssertTabExists(
                tabId,
                tab => tab
                    .ServedBeverages
                    .All(sb => beverageMenuNumbers.Contains(sb.MenuNumber)));
        }

        [Theory]
        [AutoData]
        public async Task CanOrderBeverages(Guid tabId, List<Beverage> beverages)
        {
            // Arrange
            await AddBeveragesToDb(beverages);
            await OpenTab(tabId);

            var beveragesToOrder = beverages.Select(b => b.MenuNumber).ToArray();
            
            // Act
            var result = await _fixture.SendAsync(new OrderBeverages
            {
                TabId = tabId,
                MenuNumbers = beveragesToOrder
            });

            // Assert
            result.HasValue.ShouldBeTrue();

            await AssertTabExists(
                tabId,
                tab => tab
                    .OrderedBeverages
                    .All(b => beveragesToOrder.Contains(b.MenuNumber)));
        }

        [Theory]
        [AutoData]
        public async Task CannotOpenExistingTab(Guid tabId)
        {
            // Arrange
            await OpenTab(tabId);

            // Act
            var result = await _fixture.SendAsync(new OpenTab
            {
                TabId = tabId,
                ClientName = "Some client"
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.AlreadyExists(tabId)));
        }

        [Theory]
        [AutoData]
        public async Task CannotOrderWhenTabIsClosed(Guid tabId, int[] beverageNumbersToOrder)
        {
            // Arrange
            await OpenTab(tabId);
            await CloseTab(tabId, 0);

            // Act
            // For this test we don't care if the beverage numbers exist in the database
            // because we expect it to fail before checking
            var result = await _fixture.SendAsync(new OrderBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageNumbersToOrder
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.NotOpen(tabId)));
        }

        [Theory]
        [AutoData]
        public async Task CannotOrderUnexistingBeverages(Guid tabId, int[] beverageNumbersToOrder)
        {
            // Arrange
            await OpenTab(tabId);

            // Act
            var result = await _fixture.SendAsync(new OrderBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageNumbersToOrder
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            var expectedErrors = beverageNumbersToOrder
                .Select(Errors.Beverage.NotFound)
                .ToArray();

            result.MatchNone(error => error
                .Messages
                .ShouldAllBe(m => expectedErrors.Contains(m)));
        }

        [Theory]
        [AutoData]
        public async Task CannotOpenTabWithInvalidName(Guid tabId)
        {
            // Arrange
            var command = new OpenTab
            {
                TabId = tabId,
                ClientName = null
            };

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.InvalidClientName));
        }

        [Theory]
        [AutoData]
        public async Task CannotServeBeveragesThatAreNotOrdered(Guid tabId, int[] beverageNumbersToServe)
        {
            // Arrange
            await OpenTab(tabId);

            // Act
            var result = await _fixture.SendAsync(new ServeBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageNumbersToServe
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.TriedToServeUnorderedBeverages));
        }

        [Theory]
        [AutoData]
        public async Task CannotPayLessThanTheBill(Guid tabId, List<Beverage> beverages)
        {
            // Arrange
            await OpenTab(tabId);
            await AddBeveragesToDb(beverages);

            var beverageMenuNumbers = beverages.Select(b => b.MenuNumber).ToArray();

            await OrderBeverages(tabId, beverageMenuNumbers);
            await ServeBeverages(tabId, beverageMenuNumbers);

            // Act
            var result = await _fixture.SendAsync(new CloseTab
            {
                TabId = tabId,
                AmountPaid = 0
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.TriedToPayLessThanTheBill));
        }

        [Theory]
        [AutoData]
        public async Task CannotCloseTabTwice(Guid tabId)
        {
            // Arrange
            await OpenTab(tabId);
            await CloseTab(tabId, 0);

            // Act
            var result = await _fixture.SendAsync(new CloseTab
            {
                TabId = tabId,
                AmountPaid = 0
            });

            // Assert
            result.HasValue.ShouldBeFalse();

            result.MatchNone(error => error
                .Messages
                .ShouldContain(Errors.Tab.NotOpen(tabId)));
        }

        private async Task OrderBeverages(Guid tabId, params int[] beverageMenuNumbers)
        {
            await _fixture.SendAsync(new OrderBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageMenuNumbers
            });
        }

        private async Task ServeBeverages(Guid tabId, params int[] beverageMenuNumbers)
        {
            await _fixture.SendAsync(new ServeBeverages
            {
                TabId = tabId,
                MenuNumbers = beverageMenuNumbers
            });
        }

        private async Task AddBeveragesToDb(List<Beverage> beverages)
        {
            await _fixture.ExecuteDbContextAsync(async dbContext =>
            {
                await dbContext.Beverages.AddRangeAsync(beverages);
                await dbContext.SaveChangesAsync();
            });
        }

        private async Task OpenTab(Guid tabId)
        {
            await _fixture.SendAsync(new OpenTab
            {
                TabId = tabId,
                ClientName = $"Client {Guid.NewGuid().ToString()}"
            });
        }

        private async Task CloseTab(Guid tabId, decimal amountPaid)
        {
            await _fixture.SendAsync(new CloseTab
            {
                TabId = tabId,
                AmountPaid = amountPaid
            });
        }

        private async Task AssertTabExists(Guid tabId, Func<TabView, bool> predicate)
        {
            var tab = await _fixture.SendAsync(new GetTabView { Id = tabId });
            tab.Exists(predicate).ShouldBeTrue();
        }
    }
}