using Bar.CQRS;
using Bar.CQRS.Commands;
using Bar.CQRS.Commands.Tab;
using Bar.CQRS.Queries.Tab;
using Bar.Domain;
using Bar.Domain.Events;
using Bar.Domain.Events.Base;
using Bar.Domain.Views;
using Bar.Web.Events;
using Marten;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Optional;

namespace Bar.Web.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddCqrs(this IServiceCollection services)
        {
            services.AddScoped<IEventBus, EventBus>();

            services.AddScoped<IRequestHandler<OpenTab, Option<Unit, Error>>, TabCommandsHandler>();
            services.AddScoped<IRequestHandler<OrderBeverages, Option<Unit, Error>>, TabCommandsHandler>();
            services.AddScoped<IRequestHandler<ServeBeverages, Option<Unit, Error>>, TabCommandsHandler>();
            services.AddScoped<IRequestHandler<CloseTab, Option<Unit, Error>>, TabCommandsHandler>();

            services.AddScoped<IRequestHandler<GetTabView, Option<TabView, Error>>, TabQueriesHandler>();
        }

        public static void AddMarten(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(_ =>
            {
                var documentStore = DocumentStore.For(options =>
                {
                    var config = configuration.GetSection("EventStore");
                    var connectionString = config.GetValue<string>("ConnectionString");
                    var schemaName = config.GetValue<string>("Schema");

                    options.Connection(connectionString);
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                    options.Events.DatabaseSchemaName = schemaName;
                    options.DatabaseSchemaName = schemaName;

                    options.Events.InlineProjections.AggregateStreamsWith<Tab>();
                    options.Events.InlineProjections.Add(new TabViewProjection());

                    options.Events.AddEventType(typeof(TabOpened));
                    options.Events.AddEventType(typeof(TabClosed));
                    options.Events.AddEventType(typeof(BeveragesOrdered));
                    options.Events.AddEventType(typeof(BeveragesServed));
                });

                return documentStore.OpenSession();
            });
        }
    }
}