using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Contracts.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Assistant.Net.Scheduler.Trigger.Configuration
{

    [Obsolete("replaced with TriggerEventSource")]
    internal class TriggerConfigurationSource : IConfigurationSource
    {
        private readonly IServiceProvider provider;

        public TriggerConfigurationSource(string connectionString, string databaseName) =>
            provider = new ServiceCollection()
                .AddMessagingClient(b => b
                    .UseMongo(o => o.Connection(connectionString).Database(databaseName))
                    .AddMongo<RunQuery>())
                .AddLogging(b => b.AddConsole())
                .AddSingleton<TriggerConfigurationProvider>()
                .BuildServiceProvider();

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            provider.GetRequiredService<TriggerConfigurationProvider>();
    }
}
