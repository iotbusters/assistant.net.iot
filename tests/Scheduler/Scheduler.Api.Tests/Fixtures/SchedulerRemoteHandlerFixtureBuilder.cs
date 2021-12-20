using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerRemoteHandlerFixtureBuilder
    {
        private readonly IHostBuilder remoteHostBuilder;
        private readonly IServiceCollection services;

        public SchedulerRemoteHandlerFixtureBuilder()
        {
            services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>())
                .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
                {
                    Interval = TimeSpan.FromSeconds(0.03),
                    MaxAttemptNumber = 5
                });
            remoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, s) => new Startup(ctx.Configuration).ConfigureServices(s))
                .ConfigureServices(s => s
                    .AddMongoMessageHandling(b => b
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureMongoHandlingOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    }));
        }

        public SchedulerRemoteHandlerFixtureBuilder UseMongo(string connectionString, string database)
        {
            services
                .ConfigureMessagingClient(b => b.UseMongo(o => o.Connection(connectionString).Database(database)));
            remoteHostBuilder.ConfigureServices(s => s
                .AddStorage(b => b.UseMongo(o => o.Connection(connectionString))) // not used but dependent
                .ConfigureMongoMessageHandling(b => b.UseMongo(o => o.Connection(connectionString).Database(database))));
            return this;
        }

        public SchedulerRemoteHandlerFixtureBuilder ReplaceMongoHandler(object handler)
        {
            remoteHostBuilder.ConfigureServices(s => s.ConfigureMongoMessageHandling(b => b.AddHandler(handler)));

            var messageType = handler.GetType().GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                              ?? throw new ArgumentException("Invalid message handler type.", nameof(handler));
            services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public SchedulerRemoteHandlerFixture Build()
        {
            var provider = services.BuildServiceProvider();
            var host = remoteHostBuilder.Start();
            return new SchedulerRemoteHandlerFixture(provider, host);
        }
    }
}
