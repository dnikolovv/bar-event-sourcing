using Bar.CQRS.Commands;
using Bar.CQRS.Commands.Base;
using Bar.CQRS.Commands.Tab;
using Bar.Data;
using Bar.Domain;
using Bar.Domain.Errors;
using Bar.Domain.Events.Base;
using Marten;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Optional;
using Optional.Async;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bar.CQRS
{
    public class TabCommandsHandler :
        ICommandHandler<OpenTab>,
        ICommandHandler<CloseTab>,
        ICommandHandler<ServeBeverages>,
        ICommandHandler<OrderBeverages>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventBus _eventBus;
        private readonly IDocumentSession _session;

        public TabCommandsHandler(IDocumentSession documentSession, ApplicationDbContext dbContext, IEventBus eventBus)
        {
            _session = documentSession;
            _dbContext = dbContext;
            _eventBus = eventBus;
        }

        public Task<Option<Unit, Error>> Handle(OrderBeverages request, CancellationToken cancellationToken) =>
            ValidateCommandIsNotEmpty(request).FlatMapAsync(command =>
            GetTabIfNotClosed(command.TabId, cancellationToken).FlatMapAsync(tab =>
            GetBeveragesIfInStock(command.MenuNumbers).MapAsync(beveragesToOrder =>
            PublishEvents(tab.Id, tab.OrderBeverages(beveragesToOrder)))));

        public Task<Option<Unit, Error>> Handle(ServeBeverages request, CancellationToken cancellationToken) =>
            ValidateCommandIsNotEmpty(request).FlatMapAsync(command =>
            AssureAllBeveragesAreOutstanding(command, cancellationToken).FlatMapAsync(tab =>
            GetBeveragesIfInStock(command.MenuNumbers).MapAsync(beveragesToServe =>
            PublishEvents(tab.Id, tab.ServeBeverages(beveragesToServe)))));

        public Task<Option<Unit, Error>> Handle(CloseTab request, CancellationToken cancellationToken) =>
            ValidateCommandIsNotEmpty(request).FlatMapAsync(command =>
            GetTabIfNotClosed(command.TabId, cancellationToken).
            FilterAsync(async tab => command.AmountPaid >= tab.ServedItemsValue, Errors.Tab.TriedToPayLessThanTheBill).MapAsync(tab =>
            PublishEvents(tab.Id, tab.CloseTab(command.AmountPaid))));

        public Task<Option<Unit, Error>> Handle(OpenTab request, CancellationToken cancellationToken) =>
            ValidateCommandIsNotEmpty(request).
            Filter(r => !string.IsNullOrEmpty(r.ClientName), Errors.Tab.InvalidClientName).FlatMapAsync(command =>
            TabShouldNotExist(command.TabId, cancellationToken).MapAsync(tab =>
            PublishEvents(tab.Id, tab.OpenTab(command.ClientName))));

        private Task<Option<Tab, Error>> AssureAllBeveragesAreOutstanding(ServeBeverages command, CancellationToken cancellationToken) =>
            GetTabIfNotClosed(command.TabId, cancellationToken).
            FilterAsync(async tab =>
            {
                var outstandingMenuNumbers = tab
                    .OutstandingBeverages
                    .ToLookup(x => x.MenuNumber);

                return command
                    .MenuNumbers
                    .All(num => outstandingMenuNumbers.Contains(num));
            }, Errors.Tab.TriedToServeUnorderedBeverages);

        private async Task<Option<List<Beverage>, Error>> GetBeveragesIfInStock(IEnumerable<int> menuNumbers)
        {
            var allBeveragesInStock = (await EntityFrameworkQueryableExtensions.ToListAsync(_dbContext.Beverages))
                .ToDictionary(x => x.MenuNumber);

            var beveragesToServeResult = menuNumbers
                .Select(menuNumber => allBeveragesInStock
                    .GetValueOrNone(menuNumber)
                    .WithException<Error>(Errors.Beverage.NotFound(menuNumber)))
                .ToList();

            var errors = beveragesToServeResult.Exceptions().ToArray();

            return errors.Length > 0 ?
                Option.None<List<Beverage>, Error>(errors) :
                beveragesToServeResult.Values().ToList().Some<List<Beverage>, Error>();
        }

        private Task<Tab> GetTabFromStore(Guid id, CancellationToken cancellationToken) =>
            _session.LoadAsync<Tab>(id, cancellationToken);

        private Task<Option<Tab, Error>> GetTabIfExists(Guid id, CancellationToken cancellationToken) =>
            GetTabFromStore(id, cancellationToken)
                .SomeNotNull<Tab, Error>(Errors.Tab.NotFound(id));

        private Task<Option<Tab, Error>> GetTabIfNotClosed(Guid id, CancellationToken cancellationToken) =>
            GetTabIfExists(id, cancellationToken)
                .FilterAsync(async tab => tab.IsOpen, Errors.Tab.NotOpen(id));

        private async Task<Unit> PublishEvents(Guid tabId, params IEvent[] events)
        {
            _session.Events.Append(tabId, events);
            await _session.SaveChangesAsync();
            await _eventBus.Publish(events);

            return Unit.Value;
        }

        private Task<Option<Tab, Error>> TabShouldNotExist(Guid id, CancellationToken cancellationToken) =>
            GetTabFromStore(id, cancellationToken)
                .SomeWhen<Tab, Error>(t => t == null, Errors.Tab.AlreadyExists(id))
                .MapAsync(async _ => new Tab(id));

        private static Option<TCommand, Error> ValidateCommandIsNotEmpty<TCommand>(TCommand command) where TCommand : TabCommand =>
            command
                .SomeNotNull<TCommand, Error>(Errors.Generic.NullCommand)
                .Filter(r => r.TabId != Guid.Empty, Errors.Tab.InvalidId);
    }
}