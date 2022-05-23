using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Trigger.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Tests.Fixtures;

public class SchedulerRemoteHandlerFixture : IDisposable
{
    private readonly TestTriggerQueriesHandler triggerHandler;
    private readonly TestConfigureOptionsSource remoteSource;
    private readonly TestConfigureOptionsSource clientSource;
    public readonly ServiceProvider provider;
    private readonly IHost host;

    public SchedulerRemoteHandlerFixture(
        TestTriggerQueriesHandler triggerHandler,
        TestConfigureOptionsSource remoteSource,
        TestConfigureOptionsSource clientSource,
        ServiceProvider provider,
        IHost host)
    {
        this.triggerHandler = triggerHandler;
        this.remoteSource = remoteSource;
        this.clientSource = clientSource;
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public void ReplaceHandlers(params object[] handlerInstances)
    {
        remoteSource.Configurations.Add(o =>
        {
            triggerHandler.Clear();
            o.Handlers.Clear();
            o.AddHandler(triggerHandler);
            foreach (var handlerInstance in handlerInstances)
            {
                o.AddHandler(handlerInstance);

                var handlerType = handlerInstance.GetType();
                var messageType = handlerType.GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(handlerInstances));
                triggerHandler.Add(messageType);
            }
        });
        clientSource.Configurations.Add(o =>
        {
            o.Handlers.Clear();
            foreach (var handlerInstance in handlerInstances)
            {
                var handlerType = handlerInstance.GetType();
                var messageType = handlerType.GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(handlerInstances));
                o.AddMongo(messageType);
            }
        });
        remoteSource.Reload();
        clientSource.Reload();
    }

    public T Service<T>() where T : class => provider.GetRequiredService<T>();
    public T RemoteService<T>() where T : class => host.Services.GetRequiredService<T>();

    public void Dispose()
    {
        host.Dispose();
        provider.Dispose();
    }
}