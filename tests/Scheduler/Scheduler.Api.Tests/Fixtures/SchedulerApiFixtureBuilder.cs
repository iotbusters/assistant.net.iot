using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using Assistant.Net.Scheduler.Contracts;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerApiFixtureBuilder
    {
        public SchedulerApiFixtureBuilder()
        {
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .UseStartup<Startup>()
                .ConfigureServices(s => s
                    .AddStorage(b => b
                        .AddLocal<Guid, AutomationModel>()
                        .AddLocal<Guid, JobModel>())
                    .ConfigureMessageClient(b => b.ClearInterceptors())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public SchedulerApiFixtureBuilder Add<TRequest, TResponse>(IMessageHandler<TRequest, TResponse> handler)
            where TRequest : IMessage<TResponse>
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMessageClient(b => b.AddLocal<IMessageHandler<TRequest, TResponse>>())
                .ReplaceSingleton(_ => handler));
            return this;
        }

        public SchedulerApiFixture Build()
        {
            var host = RemoteHostBuilder.Start();
            var provider = new ServiceCollection()
                .AddSingleton(host)// to dispose all at once
                .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
                //.AddJsonSerialization()
                .BuildServiceProvider();
            return new SchedulerApiFixture(provider);
        }
    }
}
