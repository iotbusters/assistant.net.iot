using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.Trigger.Options;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public class SchedulerRemoteHandlerFixtureBuilder
{
    private readonly TestTriggerQueriesHandler triggerHandler = new();
    private readonly TestConfigureOptionsSource remoteSource = new();
    private readonly TestConfigureOptionsSource clientSource = new();
    private readonly IHostBuilder remoteHostBuilder;
    private readonly IServiceCollection services;

    public SchedulerRemoteHandlerFixtureBuilder()
    {
        services = new ServiceCollection()
            .AddMessagingClient(b => b
                .TimeoutIn(TimeSpan.FromSeconds(0.5))
                .ClearInterceptors())
            .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.01),
                MaxAttemptNumber = 10
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, remoteServices) =>
            {
                new Startup(context.Configuration).ConfigureServices(remoteServices);
                remoteServices
                    .ConfigureMongoMessageHandling(b => b
                        .TimeoutIn(TimeSpan.FromSeconds(0.5)))
                    .ConfigureMongoHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    })
                    .ConfigureMessagingClient(b => b
                        .TimeoutIn(TimeSpan.FromSeconds(0.5))
                        .ClearInterceptors()
                        .AddHandler(triggerHandler))
                    .Configure<TriggerPollingOptions>(o =>
                    {
                        o.PollingWait = TimeSpan.FromSeconds(0.001);
                        //o.PollingTimeout = TimeSpan.FromSeconds(10);
                    });
            });
    }

    public SchedulerRemoteHandlerFixtureBuilder UseMongo(string connectionString, string database)
    {
        var configureMessaging = new Action<MongoOptions>(o => o.Connection(connectionString).Database(database));

        services
            .ConfigureMessagingClient(b => b.UseMongo(configureMessaging))
            .BindOptions(clientSource);
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureMongoMessageHandling(b => b.UseMongo(configureMessaging))
            .ConfigureMessagingClient(b => b.UseMongo(configureMessaging))
            .BindOptions(MongoOptionsNames.DefaultName, remoteSource));
        return this;
    }

    public SchedulerRemoteHandlerFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                          ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

        remoteSource.Configurations.Add(o =>
        {
            triggerHandler.Add(messageType);
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o => o.AddMongo(messageType));
        return this;
    }

    public SchedulerRemoteHandlerFixture Build()
    {
        var host = remoteHostBuilder.Start();
        var provider = services.BuildServiceProvider();
        return new SchedulerRemoteHandlerFixture(triggerHandler, remoteSource, clientSource, provider, host);
    }
}
