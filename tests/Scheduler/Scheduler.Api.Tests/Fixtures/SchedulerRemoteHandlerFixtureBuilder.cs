using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerRemoteHandlerFixtureBuilder
    {
        public SchedulerRemoteHandlerFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>())
                .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
                {
                    Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 3
                });
            RemoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(b => b.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings__StorageDatabase", "mongodb://"),
                    new KeyValuePair<string, string>("ConnectionStrings__RemoteMessageHandler", "mongodb://")
                }))
                .ConfigureServices((ctx, s) => new Startup(ctx.Configuration).ConfigureServices(s))
                .ConfigureServices(s => s
                    .ConfigureMessagingClient(b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureMongoHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    }));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }
        public IServiceCollection Services { get; init; }

        public SchedulerRemoteHandlerFixtureBuilder UseMongo(string connectionString, string database)
        {
            Services
                .ConfigureMessagingClient(b => b.UseMongo(o => o.ConnectionString = connectionString))
                .ConfigureMongoHandlingClientOptions(o => o.DatabaseName = database);
            RemoteHostBuilder.ConfigureServices(s => s
                .AddStorage(b => b.UseMongo(o => o.ConnectionString = connectionString)) // not used but dependent
                .ConfigureMessagingClient(b => b.UseMongo(o => o.ConnectionString = connectionString))
                .ConfigureMongoHandlingServerOptions(o => o.DatabaseName = database));
            return this;
        }

        public SchedulerRemoteHandlerFixtureBuilder ReplaceMongoHandler(IAbstractHandler handler)
        {
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClient(b => b.AddLocalHandler(handler)));

            var messageType = handler.GetType().GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                              ?? throw new ArgumentException("Invalid message handler type.", nameof(handler));
            Services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public SchedulerRemoteHandlerFixture Build()
        {
            var provider = Services.BuildServiceProvider();
            var host = RemoteHostBuilder.Start();
            return new SchedulerRemoteHandlerFixture(provider, host);
        }
    }
}
