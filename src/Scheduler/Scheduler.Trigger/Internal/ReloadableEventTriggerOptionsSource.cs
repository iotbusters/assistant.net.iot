using Assistant.Net.Abstractions;
using Assistant.Net.Scheduler.Trigger.Options;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Trigger.Internal;

/// <summary>
///     Known message type handling configuration options source implementation.
/// </summary>
internal sealed class ReloadableEventTriggerOptionsSource : ConfigureOptionsSourceBase<EventTriggerOptions>
{
    private static IDictionary<Type, IList<Guid>> knownEventTriggers = new Dictionary<Type, IList<Guid>>();

    public override void Configure(EventTriggerOptions options) =>
        options.EventTriggers = knownEventTriggers;

    /// <summary>
    ///     Triggers message type handling configuration renewal.
    /// </summary>
    /// <param name="eventTriggers">Known event-message types and associated run IDs to handle.</param>
    public void Reload(IDictionary<Type, IList<Guid>> eventTriggers)
    {
        knownEventTriggers = eventTriggers;

        Reload();
    }

    /// <summary>
    ///     Triggers message type handling configuration renewal.
    /// </summary>
    /// <param name="eventType">Known event-message types.</param>
    /// <param name="runId">Associated run ID.</param>
    public void Add(Type eventType, Guid runId)
    {
        if (knownEventTriggers.TryGetValue(eventType, out var list))
            list.Add(runId);
        else
            knownEventTriggers.Add(eventType, new List<Guid> {runId});

        Reload();
    }
}
