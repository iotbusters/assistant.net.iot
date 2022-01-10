using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Trigger.Abstractions;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Trigger.Internal;

public class DefaultMessageHandlerFactory : IMessageHandlerFactory
{
    private readonly IServiceProvider provider;

    public DefaultMessageHandlerFactory(IServiceProvider provider) =>
        this.provider = provider;

    public object Create(Type messageType)
    {
        var handlerType = typeof(GenericMessageHandler<>).MakeGenericType(messageType);
        return ActivatorUtilities.CreateInstance(provider, handlerType);
    }
}
