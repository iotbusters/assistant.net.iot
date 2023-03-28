using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Scheduler.EventHandler.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;

public sealed class SchedulerRemoteEventHandlerFixtureBuilder
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
                .TimeoutIn(TimeSpan.FromSeconds(5))
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>())
            .BindOptions(source)
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.1),
                MaxAttemptNumber = 100
            });
        remoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, s) =>
            {
                ctx.HostingEnvironment.ApplicationName = typeof(Startup).Assembly.GetName().Name!;
                new Startup(ctx.Configuration).ConfigureServices(s);
            })
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o
                    .Exclude("Newtonsoft")
                    .Exclude("NUnit")
                    .Exclude("MongoDB")
                    .Exclude("SharpCompress"))
                .AddGenericMessageHandling()
                .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
                    .UseLocalSingleProvider()
                    .TimeoutIn(TimeSpan.FromSeconds(5))
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
                }));
    }

    public SchedulerRemoteEventHandlerFixtureBuilder UseSqlite(string connectionString)
    {
        services
            .ConfigureMessagingClient(b => b
            .UseSqlite(o => o.Connection(connectionString))
            .UseSqliteSingleProvider());
        remoteHostBuilder.ConfigureServices(s => s
            .ConfigureGenericMessageHandling(b => b
                .UseSqlite(o => o.Connection(connectionString))));
        return this;
    }

    public SchedulerRemoteEventHandlerFixtureBuilder ReplaceRemoteHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerRemoteEventHandlerFixture Build()
    {
        var host = remoteHostBuilder.Start();
        var provider = services
            .AddSystemLifetime(_ => host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping)
            .BuildServiceProvider();

        source.ConfigureAction = o =>
        {
            var ro = host.Services.GetRequiredService<IOptionsSnapshot<MessagingClientOptions>>().Get(GenericOptionsNames.DefaultName);
            foreach (var messageType in ro.Handlers.Keys)
                o.AddGeneric(messageType);
        };

        var storage = provider.GetRequiredService<IAdminStorage<string, RemoteHandlerModel>>();
        SpinWait.SpinUntil(() =>
        {
            var task = storage.GetKeys().ToArrayAsync();
            task.AsTask().Wait();
            return task.Result.Length > 0;
        });

        return new SchedulerRemoteEventHandlerFixture(provider, host);
    }
}
