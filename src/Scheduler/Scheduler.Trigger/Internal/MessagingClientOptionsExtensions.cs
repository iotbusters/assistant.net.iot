using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
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
    public static MessagingClientOptions AddGenericHandlers(this MessagingClientOptions options, IDictionary<Guid, Type> messageTypes)
    {
        foreach (var runId in messageTypes.Keys)
        {
            var messageType = messageTypes[runId];
            if (!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            options.Handlers[messageType] = new HandlerDefinition(p =>
            {
                var providerType = typeof(GenericMessageHandler<>).MakeGenericTypeBoundToMessage(messageType);
                var provider = ActivatorUtilities.CreateInstance(p, providerType, runId);
                return (IAbstractHandler)provider;
            });
        }

        return options;
    }
}
