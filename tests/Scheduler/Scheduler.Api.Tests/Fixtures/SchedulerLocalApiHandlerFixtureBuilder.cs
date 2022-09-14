using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Api.Tests.Fixtures;

public sealed class SchedulerLocalApiHandlerFixtureBuilder
{
    private readonly ServiceCollection services;
    private Action<IServiceProvider> configureProvider = delegate { };

    public SchedulerLocalApiHandlerFixtureBuilder()
    {
        services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        new Startup(configuration).ConfigureServices(services);
        services
            .AddTypeEncoder(o => o
                .Exclude("Newtonsoft")
                .Exclude("NUnit")
                .Exclude("MongoDB")
                .Exclude("SharpCompress"))
            .ConfigureGenericMessageHandling(b => b.UseLocal());
    }

    public SchedulerLocalApiHandlerFixtureBuilder Store<TKey, TValue>(TKey key, TValue value)
    {
        configureProvider += p =>
        {
            var storage = p.GetService<IStorage<TKey, TValue>>();
            storage.Should().NotBeNull("Unknown storage");
            storage!.AddOrUpdate(key, value).GetAwaiter().GetResult();
        };
        return this;
    }

    public SchedulerLocalApiHandlerFixtureBuilder ReplaceStorage<TKey, TValue>(IAdminStorage<TKey,TValue> storage)
    {
        services.ReplaceSingleton<IStorage<TKey, TValue>>(_ => storage);
        services.ReplaceSingleton(_ => storage);
        return this;
    }

    public SchedulerLocalApiHandlerFixtureBuilder ReplaceHandler(object handler)
    {
        services.ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b.AddHandler(handler));
        return this;
    }

    public SchedulerLocalApiHandlerFixture Build()
    {
        var provider = services.BuildServiceProvider();
        provider.ConfigureNamedOptionContext(GenericOptionsNames.DefaultName);

        configureProvider(provider);

        return new SchedulerLocalApiHandlerFixture(provider);
    }
}
