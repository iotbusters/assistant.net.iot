using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Scheduler.Trigger.Internal;

/// <summary>
///     Messaging client options extensions.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Registers a generic message handler definition.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddEventHandlerOf(this MessagingClientOptions options, params Type[] messageTypes)
    {
        foreach (var messageType in messageTypes)
        {
            if (!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var providerType = typeof(EventTriggeredHandler);
                var provider = ActivatorUtilities.CreateInstance(p, providerType);
                return (IAbstractHandler)provider;
            });
        }

        return options;
    }
}
