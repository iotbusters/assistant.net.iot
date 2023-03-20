using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     Automation run trigger deactivated event.
/// </summary>
public class TriggerDeactivatedEvent : IMessage
{
    /// <summary/>
    public TriggerDeactivatedEvent(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Associated run ID.
    /// </summary>
    public Guid RunId { get; }
}
