using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.Api.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public class SchedulerRemoteApiHandlerFixtureBuilder
{
    private readonly IHostBuilder remoteHostBuilder;
    private readonly IServiceCollection services;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> source = new();

    public SchedulerRemoteApiHandlerFixtureBuilder()
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
                Interval = TimeSpan.FromSeconds(0.1), MaxAttemptNumber = 10
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, s) => new Startup(ctx.Configuration).ConfigureServices(s))
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o
                    .Exclude("Newtonsoft")
                    .Exclude("NUnit")
                    .Exclude("MongoDB")
                    .Exclude("SharpCompress"))
                .ConfigureGenericMessagingClient(b => b
                    .DebuggerTimeout()
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>())
                .ConfigureGenericHandlingServerOptions(o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.05);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.01);
                }));
    }

    public SchedulerRemoteApiHandlerFixtureBuilder UseSqlite(string connectionString)
    {
        services.ConfigureMessagingClient(b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider());
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureGenericMessageHandling(b => b
                .UseSqlite(o => o.Connection(connectionString))));
        return this;
    }

    public SchedulerRemoteApiHandlerFixtureBuilder ReplaceHandler(object handlerInstance)
    {
        services.ConfigureMessagingClient(b =>
        {
            var handlerType = handlerInstance.GetType();
            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
            foreach (var messageType in messageTypes)
                b.AddSingle(messageType);
        });
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureGenericMessageHandling(b => b.AddHandler(handlerInstance)));
        return this;
    }

    public SchedulerRemoteApiHandlerFixture Build()
    {
        var provider = services.BuildServiceProvider();
        var host = remoteHostBuilder.Start();

        source.ConfigureAction = o =>
        {
            var ro = host.Services.GetRequiredService<IOptionsSnapshot<MessagingClientOptions>>().Get(GenericOptionsNames.DefaultName);
            foreach (var messageType in ro.Handlers.Keys)
                o.AddGeneric(messageType);
        };

        return new SchedulerRemoteApiHandlerFixture(provider, host);
    }
}
