using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net.Http;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures
{
    public class SchedulerApiFixtureBuilder
    {
        public SchedulerApiFixtureBuilder()
        {
            RemoteHostBuilder = Host.CreateDefaultBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .UseStartup<Startup>()
                .ConfigureServices(s =>
                {
                    // disable interceptors
                    s.ConfigureMessagingClient(b => b.ClearInterceptors());

                    // disable MongoDB event handler
                    var serviceDescriptor = s.Single(x =>
                        x.ServiceType == typeof(IHostedService) && x.ImplementationType?.Name == "MessageHandlingService");
                    s.Remove(serviceDescriptor);
                }));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public SchedulerApiFixtureBuilder ReplaceMongoHandler(IAbstractHandler handler)
        {
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClient(b => b.AddLocalHandler(handler)));
            return this;
        }

        public SchedulerApiFixture Build()
        {
            var host = RemoteHostBuilder.Start();
            var provider = new ServiceCollection()
                .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
                //.AddJsonSerialization()
                .BuildServiceProvider();
            return new SchedulerApiFixture(provider, host);
        }
    }
}
