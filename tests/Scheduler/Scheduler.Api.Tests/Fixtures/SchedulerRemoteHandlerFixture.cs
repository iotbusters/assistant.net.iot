using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public class SchedulerRemoteHandlerFixture : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public SchedulerRemoteHandlerFixture(ServiceProvider provider, IHost host)
    {
        this.provider = provider;
        this.host = host;
    }

    public async Task<TResponse> Handle<TResponse>(IMessage<TResponse> request)
    {
        var handler = provider.GetService<IMessagingClient>();
        handler.Should().NotBeNull();

        return await handler!.Request(request);
    }

    public void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }
}