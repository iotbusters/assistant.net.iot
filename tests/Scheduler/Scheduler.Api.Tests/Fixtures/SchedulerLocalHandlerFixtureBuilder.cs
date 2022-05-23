using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging;
using Assistant.Net.Scheduler.Contracts.Models;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public class SchedulerLocalHandlerFixtureBuilder
{
    private readonly ServiceCollection services;

    public SchedulerLocalHandlerFixtureBuilder()
    {
        services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        new Startup(configuration).ConfigureServices(services);
        services
            .ConfigureMessagingClient(b => b
                .ClearInterceptors())
            // add correlation context
            .AddDiagnosticContext(_ => Guid.NewGuid().ToString())
            // override original provider
            .AddStorage(b => b
                .AddLocal<Guid, AutomationModel>()
                .AddLocal<Guid, JobModel>()
                .AddLocal<Guid, TriggerModel>()
                .AddLocal<Guid, RunModel>());
    }

    public SchedulerLocalHandlerFixtureBuilder AddStorage<TKey, TValue>(IAdminStorage<TKey,TValue> storage) where TKey : struct
    {
        services.ReplaceSingleton<IStorage<TKey, TValue>>(_ => storage);
        services.ReplaceSingleton(_ => storage);
        return this;
    }

    public SchedulerLocalHandlerFixtureBuilder ReplaceHandler(object handler)
    {
        services.ConfigureMessagingClient(b => b.AddHandler(handler));
        return this;
    }

    public SchedulerLocalHandlerFixture Build()
    {
        var provider = services.BuildServiceProvider();
        return new SchedulerLocalHandlerFixture(provider);
    }
}
