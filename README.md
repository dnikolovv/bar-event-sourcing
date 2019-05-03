[![Build Status](https://travis-ci.org/dnikolovv/bar-event-sourcing.svg?branch=master)](https://travis-ci.org/dnikolovv/bar-event-sourcing)

# Bar

A sample .NET application featuring domain-driven design, CQRS, event-sourcing and a bit of SignalR. It resembles an actual bar. You can open a tab, order beverages, have them served and close the tab. It has a very simple (and very, very ugly) AngularJs interface so you can run it locally and start clicking around.

## Motivation

I've always been against the "traditional" way of doing CQRS with  `void` command handlers that throw exceptions for simple business logic validations (eg. `TabClosedException`). This application started off as a quick test of how you could avoid that by having your handlers use the [`Either` monad](https://devadventures.net/2018/09/20/real-life-examples-of-functional-c-sharp-either/) and it kind of grew to something that I think is a decent*ish* example of the idea, so I'm sharing it here.

## What to look for

One of the things that is [supposed to be] interesting is the [`TabCommandsHandler.cs`](https://github.com/dnikolovv/bar-event-sourcing/blob/master/Bar.CQRS/TabCommandsHandler.cs).

Each handler is implemented as a chain of functions (using [`Optional.Async`](https://github.com/dnikolovv/optional-async)). Each function represents an operation that can either pass (continue the execution) or fail (return an `Error` to the consumer).

The chain itself contains all of the business validations such as checking whether the tab is closed, checking whether you're not serving beverages that haven't been ordered, etc.

> Note that the Either monad in this case is called `Option`.

Example:

```csharp
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
```

Besides looking very neat and being easy to read (if we ignore the long ass `FlatMapAsync` calls), another benefit that this gives us is that is makes the testing part easier. Since there is one function call for every operation that can potentially fail, you can simply check if you have at least one integration test per line.

The application has a complete suite of integration tests that make it very clear what it's all about. (see [`TabTests.cs`](https://github.com/dnikolovv/bar-event-sourcing/blob/master/Bar.Tests/Tab/TabTests.cs) and [`BarTests.cs`](https://github.com/dnikolovv/bar-event-sourcing/blob/master/Bar.Tests/Bar/BarTests.cs))

Example:

```csharp
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
```

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

If you have Docker installed you can just run `docker-compose up`.

If not, you'll need to have [PostgreSql](https://www.postgresql.org/download/) either installed locally or at least have some instance available to set up the connection strings.

You'll also need at least version `2.2` of the [`.NET Core SDK`](https://dotnet.microsoft.com/download).

### Running

> Note that you can point both the event-store and the relational connection to the same database

#### Using Docker

1. Execute `run-app.sh`

#### Using Visual Studio

1. Open the `.sln` file using Visual Studio
2. Set up the connection strings inside `Bar.Web/appsettings.json`
3. Execute `Update-Database` inside the `Package Manager Console`
4. Run the application

#### Using the `dotnet` CLI

1. Open the project folder inside your favorite editor
2. Set up the connection strings inside `Bar.Web/appsettings.json`
3. Execute `dotnet ef database update` inside the `Bar.Web` folder
4. Execute `dotnet run`
5. Go to `https://localhost:5001` (or whatever port you're running it on)

You should see the "open tab" screen.

![open tab](https://devadventures.net/wp-content/uploads/2018/12/open-tab-screen.png)

## Running the tests

> Note that you can point both the event-store and the relational connection to the same database

#### Using Docker

1. Simply run `run-tests.sh`.

#### Using Visual Studio or the `dotnet` CLI

1. Set up the connection strings inside `Bar.Tests/appsettings.json` to a valid database. (if you point it to an unexisting one, the app will create it for you)
2. Either run them through the `Test Explorer` in Visual Studio or using `dotnet test`

## Contributing

If you feel like contributing, PRs are welcome!

## License

This project is licensed under the MIT License.

## Acknowledgments

Jimmy Bogard for his awesome [MediatR](https://github.com/jbogard/MediatR) library, all of the helpful articles and code samples. You're truly an inspiration :).

Nils Luck for [Optional](https://github.com/nlkl/Optional).
