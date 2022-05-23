using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     The run has been successfully completed event.
/// </summary>
public class RunSucceededEvent : IMessage
{
    /// <summary/>
    public RunSucceededEvent(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Associated run ID.
    /// </summary>
    public Guid RunId { get; set; }
}