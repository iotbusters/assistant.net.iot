using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace Assistant.Net.Test.Common.Fixtures;

public sealed class RemoteHandlerFixture : IDisposable
{
    private readonly ServiceProvider clientProvider;
    private readonly IHost host;

    public RemoteHandlerFixture(ServiceProvider clientProvider, IHost host)
    {
        this.clientProvider = clientProvider;
        this.host = host;
        var availabilityService = HostService<ServerAvailabilityService>();
        availabilityService.Register(timeToLive: TimeSpan.FromSeconds(1), default);
        var activityService = HostService<ServerActivityService>();
        SpinWait.SpinUntil(() => activityService.IsActivationRequested, timeout: TimeSpan.FromSeconds(1));
    }

    public IMessagingClient Client => clientProvider.GetRequiredService<IMessagingClient>();

    public T ClientService<T>() where T : class => clientProvider.GetRequiredService<T>();

    public T HostService<T>() where T : class => host.Services.GetRequiredService<T>();

    public void Dispose()
    {
        host.StopAsync(timeout: TimeSpan.FromSeconds(1)).ConfigureAwait(false).GetAwaiter().GetResult();
        host.Dispose();
        clientProvider.Dispose();
    }
}
