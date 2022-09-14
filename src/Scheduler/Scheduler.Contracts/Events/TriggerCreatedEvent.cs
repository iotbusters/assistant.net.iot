using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     Automation run trigger created event.
/// </summary>
public class TriggerCreatedEvent : IMessage
{
    /// <summary/>
    public TriggerCreatedEvent(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Associated run ID.
    /// </summary>
    public Guid RunId { get; }
}
