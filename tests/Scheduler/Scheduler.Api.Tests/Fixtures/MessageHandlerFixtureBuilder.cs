using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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