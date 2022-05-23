using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     The run has failed event.
/// </summary>
public class RunFailedEvent : IMessage
{
    /// <summary/>
    public RunFailedEvent(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Associated run ID.
    /// </summary>
    public Guid RunId { get; set; }
}