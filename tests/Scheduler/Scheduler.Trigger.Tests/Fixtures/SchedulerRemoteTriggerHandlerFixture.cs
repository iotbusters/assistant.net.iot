using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public sealed class SchedulerRemoteTriggerHandlerFixture : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public SchedulerRemoteTriggerHandlerFixture(ServiceProvider provider, IHost host)
    {
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public T Service<T>() where T : class => host.Services.GetRequiredService<T>();

    public void Dispose()
    {
        host.StopAsync(timeout: TimeSpan.FromSeconds(1)).ConfigureAwait(false).GetAwaiter().GetResult();
        host.Dispose();
        provider.Dispose();
    }
}
