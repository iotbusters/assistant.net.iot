using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public sealed class SchedulerApiFixtureBuilder
{
    private readonly IHostBuilder remoteHostBuilder;

    public SchedulerApiFixtureBuilder()
    {
        remoteHostBuilder = Host.CreateDefaultBuilder().ConfigureWebHost(wb => wb
            .UseTestServer()
            .UseStartup<Startup>()
            .ConfigureServices(s =>
            {
                // disable interceptors
                s.ConfigureMessagingClient(b => b
                    .TimeoutIn(TimeSpan.FromSeconds(1))
                    .DebuggerTimeout()
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>());
                s.AddTypeEncoder(o => o
                    .Exclude("Newtonsoft")
                    .Exclude("NUnit")
                    .Exclude("MongoDB")
                    .Exclude("SharpCompress"));

                // disable MongoDB event handler
                s.RemoveAll(typeof(IHostedService));
            }));
    }

    public SchedulerApiFixtureBuilder ReplaceApiHandler(object handler)
    {
        remoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClient(b => b.AddHandler(handler)));
        return this;
    }

    public SchedulerApiFixture Build()
    {
        var host = remoteHostBuilder.Start();
        var provider = new ServiceCollection()
            .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
            .AddSerializer(b => b.AddJsonTypeAny())
            .BuildServiceProvider();
        return new SchedulerApiFixture(provider, host);
    }
}
