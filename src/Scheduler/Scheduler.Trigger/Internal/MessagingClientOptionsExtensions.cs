using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Scheduler.Trigger.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
    public static MessagingClientOptions AddTriggerEventHandlers(this MessagingClientOptions options, IDictionary<Guid, Type> messageTypes)
    {
        foreach (var (runId, messageType) in messageTypes)
        {
            if (!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            {
                var providerType = typeof(TriggerEventHandler);
                var provider = ActivatorUtilities.CreateInstance(p, providerType, runId);
                return (IAbstractHandler)provider;
            });
        }

        return options;
    }
}
