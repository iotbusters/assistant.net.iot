using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Storage.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public class SchedulerLocalApiHandlerFixture : IDisposable
{
    private readonly ServiceProvider provider;

    public SchedulerLocalApiHandlerFixture(ServiceProvider provider) =>
        this.provider = provider;

    public async Task<TResponse> Handle<TResponse>(IMessage<TResponse> request)
    {
        var handler = provider.GetService<IMessagingClient>();
        handler.Should().NotBeNull("Unknown handler");

        return await handler!.Request(request);
    }

    public async Task<TValue?> GetOrDefault<TKey, TValue>(TKey key)
    {
        var storage = provider.GetService<IStorage<TKey, TValue>>();
        storage.Should().NotBeNull("Unknown storage");

        return await storage!.GetOrDefault(key);
    }

    public virtual void Dispose() => provider.Dispose();
}
