using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using Assistant.Net.Test.Common.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Assistant.Net.Test.Common.Fixtures;

public sealed class RemoteHandlerFixtureBuilder<TStartup> where TStartup : class
{
    private short createdInstanceCount = 1;

    private Action<HostBuilderContext, IServiceCollection> hostBuilderConfigurations;
    private readonly IServiceCollection services;

    public RemoteHandlerFixtureBuilder()
    {
        services = new ServiceCollection()
            .AddTypeEncoder(o => o
                .Exclude("Newtonsoft")
                .Exclude("NUnit")
                .Exclude("MongoDB")
                .Exclude("SharpCompress"))
            .AddMessagingClient(b => b
                .TimeoutIn(TimeSpan.FromSeconds(10))
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>())
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.1), MaxAttemptNumber = 100
            });
        hostBuilderConfigurations += (ctx, s) =>
        {
            var startupType = typeof(TStartup);
            ctx.HostingEnvironment.ApplicationName = startupType.Assembly.GetName().Name + createdInstanceCount++;

            var startup = Activator.CreateInstance(startupType, ctx.Configuration);
            var configureServicesName = "ConfigureServices";
            var configureMethod = startupType.GetMethod(configureServicesName)
                                  ?? throw new MissingMethodException(startupType.Name, configureServicesName);
            configureMethod.Invoke(startup, new object[] { s });
        };
        hostBuilderConfigurations += (_, s) => s
            .AddTypeEncoder(o => o
                .Exclude("Newtonsoft")
                .Exclude("NUnit")
                .Exclude("MongoDB")
                .Exclude("SharpCompress"))
            .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
                .TimeoutIn(TimeSpan.FromSeconds(10))
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>())
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.1), MaxAttemptNumber = 100
            })
            .ConfigureGenericHandlingServerOptions(o =>
            {
                o.InactivityDelayTime = TimeSpan.FromSeconds(0.5);
                o.NextMessageDelayTime = TimeSpan.FromSeconds(0.1);
            })
            .Configure<HealthCheckPublisherOptions>(o => o.Delay = TimeSpan.FromSeconds(0.5));
    }

    public RemoteHandlerFixtureBuilder<TStartup> UseSqlite(string connectionString)
    {
        services
            .ConfigureMessagingClient(b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider());
        hostBuilderConfigurations += (_, s) => s
            .ConfigureMessagingClient(b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider())
            .ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b
                .UseSqlite(o => o.Connection(connectionString))
                .UseSqliteSingleProvider())
            .ConfigureGenericMessageHandling(b => b
                .UseSqlite(o => o.Connection(connectionString)));
        return this;
    }

    public RemoteHandlerFixtureBuilder<TStartup> ReplaceHandler(object handlerInstance)
    {
        services.ConfigureMessagingClient(b =>
        {
            var handlerType = handlerInstance.GetType();
            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
            foreach (var messageType in messageTypes)
                b.AddSingle(messageType);
        });
        hostBuilderConfigurations += (_, s) => s
            .ConfigureGenericMessageHandling(b => b.AddHandler(handlerInstance));
        return this;
    }

    public RemoteHandlerFixtureBuilder<TStartup> ReplaceLocalHandler(object handlerInstance)
    {
        hostBuilderConfigurations += (_, s) => s.ConfigureMessagingClient(b =>
            b.AddHandler(handlerInstance));
        return this;
    }

    public RemoteHandlerFixtureBuilder<TStartup> AddRegistrationOnly<TMessage>()
    {
        services.ConfigureMessagingClient(b => b.AddSingle(typeof(TMessage)));
        return this;
    }

    public RemoteHandlerFixtureBuilder<TStartup> ConfigureServer(Action<IServiceCollection> configureServices)
    {
        hostBuilderConfigurations += (_, s) => configureServices(s);
        return this;
    }

    public RemoteHandlerFixtureBuilder<TStartup> ConfigureClient(Action<IServiceCollection> configureServices)
    {
        configureServices(services);
        return this;
    }

    public RemoteHandlerFixture Build()
    {
        var host = Host.CreateDefaultBuilder().ConfigureServices(hostBuilderConfigurations).Start();
        var source = new TestConfigureOptionsSource<MessagingClientOptions>
        {
            ConfigureAction = o =>
            {
                var ro = host.Services.GetRequiredService<IOptionsSnapshot<MessagingClientOptions>>()
                    .Get(GenericOptionsNames.DefaultName);
                foreach (var messageType in ro.Handlers.Keys)
                    o.AddGeneric(messageType);
            }
        };
        var provider = services
            .AddSystemLifetime(_ => host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping)
            .AddLogging(b => b.AddProvider(host.Services.GetRequiredService<ILoggerProvider>()))
            .BindOptions(source)
            .BuildServiceProvider();

        return new RemoteHandlerFixture(provider, host);
    }

    public MultiHandlerFixture Build(short instanceCount)
    {
        var fixtures = Enumerable.Range(0, instanceCount).Select(_ => Build()).ToArray();
        return new MultiHandlerFixture(fixtures);
    }
}
