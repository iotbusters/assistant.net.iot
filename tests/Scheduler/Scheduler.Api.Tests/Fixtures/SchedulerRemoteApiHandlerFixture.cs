﻿using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public sealed class SchedulerRemoteApiHandlerFixture : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public SchedulerRemoteApiHandlerFixture(ServiceProvider provider, IHost host)
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

    public T Service<T>(string name) where T : class =>
        host.Services.CreateScopeWithNamedOptionContext(name).ServiceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        host.StopAsync(timeout: TimeSpan.FromSeconds(1)).ConfigureAwait(false).GetAwaiter().GetResult();
        host.Dispose();
        provider.Dispose();
    }
}
