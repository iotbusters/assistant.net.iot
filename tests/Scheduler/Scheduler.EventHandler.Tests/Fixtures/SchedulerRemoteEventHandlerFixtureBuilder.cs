using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;

public class SchedulerRemoteEventHandlerFixtureBuilder
{
    private readonly IHostBuilder remoteHostBuilder;
    private readonly IServiceCollection services;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> source = new();

    public SchedulerRemoteEventHandlerFixtureBuilder()
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
            .BindOptions(source)
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.03),
                MaxAttemptNumber = 5
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, s) =>
            {
                new Startup(ctx.Configuration).ConfigureServices(s);
                s.RemoveAll(typeof(IHostedService));
            })
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o
                    .Exclude("Newtonsoft")
                    .Exclude("NUnit")
                    .Exclude("MongoDB")
                    .Exclude("SharpCompress"))
                .AddGenericMessageHandling()
                .ConfigureGenericMessagingClient(b => b
                    .DebuggerTimeout()
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>())
                .ConfigureGenericHandlingServerOptions(o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                }));
    }

    public SchedulerRemoteEventHandlerFixtureBuilder UseSqlite(string connectionString)
    {
        services.ConfigureMessagingClient(b => b
            .UseSqlite(o => o.Connection(connectionString))
            .UseSqliteSingleProvider());
        remoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseSqlite(o => o.Connection(connectionString))));
        return this;
    }

    public SchedulerRemoteEventHandlerFixtureBuilder ReplaceRemoteHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessagingClient(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerRemoteEventHandlerFixture Build()
    {
        var provider = services.BuildServiceProvider();
        var host = remoteHostBuilder.Start();

        source.ConfigureAction = o =>
        {
            var ro = host.Services.GetRequiredService<IOptionsSnapshot<MessagingClientOptions>>().Get(GenericOptionsNames.DefaultName);
            foreach (var messageType in ro.Handlers.Keys)
                o.AddGeneric(messageType);
        };

        return new SchedulerRemoteEventHandlerFixture(provider, host);
    }
}
