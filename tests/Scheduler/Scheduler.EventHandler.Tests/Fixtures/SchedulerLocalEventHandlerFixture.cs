using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.EventHandler.Tests.Fixtures;

public sealed class SchedulerLocalEventHandlerFixture : IDisposable
{
    private readonly IServiceScope scope;

    public SchedulerLocalEventHandlerFixture(IServiceScope scope) =>
        this.scope = scope;

    public async Task<TResponse> Handle<TResponse>(IMessage<TResponse> request)
    {
        var handler = scope.ServiceProvider.GetService<IMessagingClient>();
        handler.Should().NotBeNull("Unknown handler.");
        return await handler!.Request(request);
    }

    public void Dispose() => scope.Dispose();
}
