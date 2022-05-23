using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     Arranged timer of the run has been triggered event.
/// </summary>
public class TimerTriggeredEvent : IMessage
{
    /// <summary/>
    public TimerTriggeredEvent(Guid runId, DateTimeOffset triggered)
    {
        RunId = runId;
        Triggered = triggered;
    }

    /// <summary>
    ///     Scheduled run ID.
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    ///     The date when the scheduled run was triggered.
    /// </summary>
    public DateTimeOffset Triggered { get; }
}
