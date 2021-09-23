using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class MessageHandlerFixtureBuilder
    {
        private readonly ServiceCollection services;

        public MessageHandlerFixtureBuilder()
        {
            services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            new Startup(configuration).ConfigureServices(services);
            services
                .ConfigureMessageClient(b => b.ClearInterceptors())
                .AddDiagnosticContext(_ => Guid.NewGuid().ToString())
                .AddStorage(b => b
                    .AddLocal<Guid, AutomationModel>()
                    .AddLocal<Guid, JobModel>());
        }

        public MessageHandlerFixtureBuilder AddStorage<TKey, TValue>(TestStorage<TKey,TValue> storage) where TKey : struct
        {
            services.ReplaceSingleton<IStorage<TKey, TValue>>(_ => storage);
            services.ReplaceSingleton<IAdminStorage<TKey, TValue>>(_ => storage);
            return this;
        }

        public MessageHandlerFixture Build()
        {
            var provider = services.BuildServiceProvider();
            return new MessageHandlerFixture(provider);
        }
    }
}
