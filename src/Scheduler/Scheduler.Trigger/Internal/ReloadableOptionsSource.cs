using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Trigger.Internal;

/// <summary>
///     Known message type handling configuration options source implementation.
/// </summary>
internal class ReloadableOptionsSource : ConfigureOptionsSourceBase<MessagingClientOptions>
{
    private readonly ILogger<ReloadableOptionsSource> logger;
    private Dictionary<Guid, Type> knownMessageTypes = new();

    public ReloadableOptionsSource(ILogger<ReloadableOptionsSource> logger) =>
        this.logger = logger;

    public override void Configure(MessagingClientOptions options)
    {
        options.Handlers.Clear();
        options.AddGenericHandlers(knownMessageTypes);
    }

    /// <summary>
    ///     Triggers message type handling configuration renewal.
    /// </summary>
    /// <param name="triggerMessageTypes">Known message types to allow their handling.</param>
    public void Reload(Dictionary<Guid, Type> triggerMessageTypes)
    {
        var unsupportedTypes = triggerMessageTypes.Where(x => !x.Value.IsAssignableTo(typeof(IMessage<None>))).ToArray();
        if (unsupportedTypes.Any())
        {
            foreach (var unsupportedType in unsupportedTypes)
                triggerMessageTypes.Remove(unsupportedType.Key);

            var unsupportedMessageTypes = unsupportedTypes.Select(x => x.Value).ToArray();
            logger.LogDebug("Found unsupported {MessageTypes} expecting a response.", (object)unsupportedMessageTypes);
        }

        knownMessageTypes = triggerMessageTypes;
        var messageTypes = triggerMessageTypes.Values.ToArray();
        logger.LogDebug("Reload {MessageTypes}.", (object)messageTypes);

        Reload();
    }
}
