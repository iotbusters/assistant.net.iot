using Assistant.Net.Messaging;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures
{
    public class SchedulerRemoteHandlerFixtureBuilder
    {
        private readonly TestTriggerQueriesHandler triggerHandler = new();
        private readonly TestMessageHandlerFactory factory = new();
        private readonly IHostBuilder remoteHostBuilder;
        private readonly IServiceCollection services;

        public SchedulerRemoteHandlerFixtureBuilder()
        {
            services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    //.RemoveInterceptor<CachingInterceptor>()
                    //.RemoveInterceptor<RetryingInterceptor>()
                    .ClearInterceptors())
                .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
                {
                    Interval = TimeSpan.FromSeconds(0.03),
                    MaxAttemptNumber = 5
                });
            remoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, remoteServices) =>
                {
                    new Startup(context.Configuration).ConfigureServices(remoteServices);
                    remoteServices
                        .ConfigureMongoMessageHandling(b => b
                            .TimeoutIn(TimeSpan.FromSeconds(0.5))
                            //.RemoveInterceptor<CachingInterceptor>()
                            //.RemoveInterceptor<RetryingInterceptor>()
                            .ClearInterceptors())
                        .ConfigureMongoHandlingServerOptions(o =>
                        {
                            o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                            o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                        })
                        .ConfigureMessagingClient(b => b
                            .TimeoutIn(TimeSpan.FromSeconds(0.5))
                            //.RemoveInterceptor<CachingInterceptor>()
                            //.RemoveInterceptor<RetryingInterceptor>()
                            .ClearInterceptors()
                            .AddHandler(triggerHandler))
                        .ReplaceSingleton<IMessageHandlerFactory>(_ => factory)
                        .Configure<TriggerPollingOptions>(o =>
                        {
                            o.PollingWait = TimeSpan.FromSeconds(0.001);
                            //o.PollingTimeout = TimeSpan.FromSeconds(10);
                        });
                });
        }

        public SchedulerRemoteHandlerFixtureBuilder UseMongo(string connectionString, string database)
        {
            services.ConfigureMessagingClient(b => b
                .UseMongo(o => o.Connection(connectionString).Database(database)));
            remoteHostBuilder.ConfigureServices(s => s
                .AddStorage(b => b.UseMongo(o => o.Connection(connectionString))) // not used but dependent.
                .ConfigureMongoMessageHandling(b => b
                    .UseMongo(o => o.Connection(connectionString).Database(database)))
                .ConfigureMessagingClient(b => b
                    .UseMongo(o => o.Connection(connectionString).Database(database))));
            return this;
        }

        public SchedulerRemoteHandlerFixtureBuilder ReplaceHandler(object handler)
        {
            var messageTypes = handler.GetType().GetMessageHandlerInterfaceTypes().Select(x=>x.GetGenericArguments().First()).ToArray();
            if (!messageTypes.Any())
                throw new ArgumentException("Invalid message handler type.", nameof(handler));

            foreach (var messageType in messageTypes)
            {
                factory.Add(messageType, handler);
                triggerHandler.Add(messageType);
                services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            }

            remoteHostBuilder.ConfigureServices(s => s.ConfigureMongoMessageHandling(b => b.AddHandler(handler)));

            return this;
        }

        public SchedulerRemoteHandlerFixture Build()
        {
            var host = remoteHostBuilder.Start();
            var provider = services.BuildServiceProvider();
            return new SchedulerRemoteHandlerFixture(triggerHandler, factory, provider, host);
        }

        public SchedulerRemoteHandlerFixtureBuilder AddInterceptor(object interceptor)
        {
            remoteHostBuilder.ConfigureServices(s => s.ConfigureMongoMessageHandling(b => b.AddInterceptor(interceptor)));
            //remoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClientOptions(b => b.AddInterceptor(interceptor)));
            //services.ConfigureMessagingClientOptions(b => b.AddInterceptor(interceptor));
            return this;
        }
    }
}
