using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Tests.Fixtures;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Fixtures
{
    public class SchedulerLocalHandlerFixtureBuilder
    {
        private readonly ServiceCollection services;

        public SchedulerLocalHandlerFixtureBuilder()
        {
            services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            new Startup(configuration).ConfigureServices(services);
            services
                .ConfigureMessagingClient(b => b.ClearInterceptors())
                // add correlation context
                .AddDiagnosticContext(_ => Guid.NewGuid().ToString())
                // override original provider
                .AddStorage(b => b
                    .AddLocal<Guid, AutomationModel>()
                    .AddLocal<Guid, JobModel>()
                    .AddLocal<Guid, RunModel>());
        }

        public SchedulerLocalHandlerFixtureBuilder AddStorage<TKey, TValue>(IAdminStorage<TKey,TValue> storage) where TKey : struct
        {
            services.ReplaceSingleton<IStorage<TKey, TValue>>(_ => storage);
            services.ReplaceSingleton(_ => storage);
            return this;
        }

        public SchedulerLocalHandlerFixtureBuilder ReplaceHandler(object handler)
        {
            // todo: remove after package upgrade.
            foreach (var handlerInterfaceType in handler.GetType().GetMessageHandlerInterfaceTypes())
            {
                var messageType = handlerInterfaceType.GetGenericArguments().First();
                var providerType = typeof(IMessageHandlingProvider<,>).MakeGenericTypeBoundToMessage(messageType);
                var providerDescriptor = services.FirstOrDefault(x => x.ServiceType == providerType);
                if (providerDescriptor != null)
                    services.Remove(providerDescriptor);
            }

            services.ConfigureMessagingClient(b => b.AddLocalHandler(handler));
            return this;
        }


        public SchedulerLocalHandlerFixture Build()
        {
            var provider = services.BuildServiceProvider();
            return new SchedulerLocalHandlerFixture(provider);
        }
    }
}
