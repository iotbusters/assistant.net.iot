using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.Trigger.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public class SchedulerRemoteTriggerHandlerFixtureBuilder
{
    private readonly IHostBuilder remoteHostBuilder;
    private readonly IServiceCollection services;

    public SchedulerRemoteTriggerHandlerFixtureBuilder()
    {
        services = new ServiceCollection()
            .AddTypeEncoder(o => o
                .Exclude("Newtonsoft")
                .Exclude("NUnit")
                .Exclude("MongoDB")
                .Exclude("SharpCompress"))
            .AddMessagingClient(b => b
                .DebuggerTimeout()
                .TimeoutIn(TimeSpan.FromSeconds(0.5))
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>())
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.03),
                MaxAttemptNumber = 5
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, remoteServices) =>
            {
                new Startup(ctx.Configuration).ConfigureServices(remoteServices);
                remoteServices
                    .AddTypeEncoder(o => o
                        .Exclude("Newtonsoft")
                        .Exclude("NUnit")
                        .Exclude("MongoDB")
                        .Exclude("SharpCompress"))
                    .ConfigureMessagingClient(b => b
                        .DebuggerTimeout()
                        .TimeoutIn(TimeSpan.FromSeconds(0.5))
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureGenericMessagingClient(b => b
                        .TimeoutIn(TimeSpan.FromSeconds(0.5))
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureGenericHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    });
            });
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder UseSqlite(string connectionString)
    {
        services
            .ConfigureMessagingClient(b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider());
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureMessagingClient(b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider())
            .ConfigureGenericMessageHandling(b => b
                .UseSqlite(o => o.Connection(connectionString))));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder ReplaceRemoteHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureMessagingClient(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder ReplaceRemoteServerHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureGenericMessageHandling(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder ReplaceTriggerHandler(Guid runId, Type messageType)
    {
        services.ConfigureMessagingClient(b => b.AddSingle(messageType));
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureMessagingClientOptions(GenericOptionsNames.DefaultName, o => o
                .AddTriggerEventHandlers(new Dictionary<Guid, Type> {{runId, messageType}})));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder AddRegistrationOnly<TMessage>()
    {
        services.ConfigureMessagingClient(b => b.AddSingle(typeof(TMessage)));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixture Build()
    {
        var host = remoteHostBuilder.Start();
        var provider = services.BuildServiceProvider();
        return new SchedulerRemoteTriggerHandlerFixture(provider, host);
    }
}
