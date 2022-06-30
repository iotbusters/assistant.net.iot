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
internal class ReloadableMessagingClientOptionsSource : ConfigureOptionsSourceBase<MessagingClientOptions>
{
    private readonly ILogger<ReloadableMessagingClientOptionsSource> logger;
    private static Dictionary<Guid, Type> knownMessageTypes = new();

    public ReloadableMessagingClientOptionsSource(ILogger<ReloadableMessagingClientOptionsSource> logger) =>
        this.logger = logger;

    public override void Configure(MessagingClientOptions options)
    {
        options.Handlers.Clear();
        options.AddTriggerEventHandlers(knownMessageTypes);
    }

    /// <summary>
    ///     Triggers message type handling configuration renewal.
    /// </summary>
    /// <param name="triggerMessageTypes">Known message types to allow their handling.</param>
    public void Reload(Dictionary<Guid, Type> triggerMessageTypes)
    {
        var unsupportedTypes = triggerMessageTypes.Where(x => !x.Value.IsAssignableTo(typeof(IMessage<Nothing>))).ToArray();
        if (unsupportedTypes.Any())
        {
            foreach (var unsupportedType in unsupportedTypes)
                triggerMessageTypes.Remove(unsupportedType.Key);

            var unsupportedMessageTypes = unsupportedTypes.Select(x => x.Value).ToArray();
            logger.LogDebug("Found unsupported {MessageTypes} expecting a response.", (object)unsupportedMessageTypes);
        }

        var added = triggerMessageTypes.Values.Except(knownMessageTypes.Values).Distinct();
        if (added.Any())
            logger.LogDebug("Start accepting {MessageTypes}.", added);

        var removed = knownMessageTypes.Values.Except(triggerMessageTypes.Values).Distinct();
        if (removed.Any())
            logger.LogDebug("Stop accepting {MessageTypes}.", removed);

        knownMessageTypes = triggerMessageTypes;
        var messageTypes = triggerMessageTypes.Values.ToArray();
        logger.LogDebug("Reloaded accepting {MessageTypes}.", (object)messageTypes);

        Reload();
    }
}
