using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public sealed class SchedulerRemoteTriggerHandlerFixtureBuilder
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
                .TimeoutIn(TimeSpan.FromSeconds(1))
                .DebuggerTimeout()
                .RemoveInterceptor<TimeoutInterceptor>()
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>())
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.1),
                MaxAttemptNumber = 100
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, remoteServices) =>
            {
                ctx.HostingEnvironment.ApplicationName = typeof(Startup).Assembly.GetName().Name;
                new Startup(ctx.Configuration).ConfigureServices(remoteServices);
                remoteServices
                    .AddTypeEncoder(o => o
                        .Exclude("Newtonsoft")
                        .Exclude("NUnit")
                        .Exclude("MongoDB")
                        .Exclude("SharpCompress"))
                    .ConfigureMessagingClient(b => b
                        .TimeoutIn(TimeSpan.FromSeconds(5))
                        .DebuggerTimeout()
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
                        .TimeoutIn(TimeSpan.FromSeconds(5))
                        .DebuggerTimeout()
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
                    {
                        Interval = TimeSpan.FromSeconds(0.1),
                        MaxAttemptNumber = 100
                    })
                    .ConfigureGenericHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.5);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.1);
                    })
                    .Configure<HealthCheckPublisherOptions>(o => o.Delay = TimeSpan.FromSeconds(0.1));
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
                .UseSqlite(connectionString)
                .UseSqliteSingleProvider())
            .ConfigureGenericMessageHandling(b => b
                .UseSqlite(o => o.Connection(connectionString)))
        );
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder ReplaceHandler(object handler)
    {
        services.ConfigureMessagingClient(b =>
        {
            var handlerType = handler.GetType();
            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
            foreach (var messageType in messageTypes)
                b.AddSingle(messageType);
        });
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureGenericMessageHandling(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerRemoteTriggerHandlerFixtureBuilder ReplaceRemoteServerHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureMessagingClient(b => b.AddHandler(handler)));
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
        var provider = services
            .AddSystemLifetime(_ => host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping)
            .BuildServiceProvider();

        Thread.Sleep(TimeSpan.FromSeconds(0.5));

        return new SchedulerRemoteTriggerHandlerFixture(provider, host);
    }
}
