using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;

public class SchedulerLocalEventHandlerFixtureBuilder
{
    private readonly ServiceCollection services;

    public SchedulerLocalEventHandlerFixtureBuilder()
    {
        services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        new Startup(configuration).ConfigureServices(services);
        services.RemoveAll(typeof(IHostedService));
        services
            .AddTypeEncoder(o => o
                .Exclude("Newtonsoft")
                .Exclude("NUnit")
                .Exclude("MongoDB")
                .Exclude("SharpCompress"))
            .ConfigureGenericMessageHandling(b => b.UseLocal());
    }

    public SchedulerLocalEventHandlerFixtureBuilder ReplaceHandler(object handler)
    {
        services.ConfigureGenericMessagingClient(b => b.AddHandler(handler));
        return this;
    }

    public SchedulerLocalEventHandlerFixture Build()
    {
        var scope = services.BuildServiceProvider().CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        return new SchedulerLocalEventHandlerFixture(scope);
    }
}
