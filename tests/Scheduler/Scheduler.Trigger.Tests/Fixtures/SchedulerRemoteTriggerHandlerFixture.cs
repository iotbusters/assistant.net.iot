using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public class SchedulerRemoteTriggerHandlerFixture : IDisposable
{
    internal readonly ServiceProvider provider;
    internal readonly IHost host;

    public SchedulerRemoteTriggerHandlerFixture(ServiceProvider provider, IHost host)
    {
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public void Dispose()
    {
        host.Dispose();
        provider.Dispose();
    }
}
