using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events;

/// <summary>
///     Arranged timer of the run has been triggered event.
/// </summary>
public sealed class TimerTriggeredEvent : IMessage
{
    /// <summary/>
    public TimerTriggeredEvent(Guid runId, DateTimeOffset arranged, DateTimeOffset scheduled, DateTimeOffset triggered)
    {
        RunId = runId;
        Arranged = arranged;
        Scheduled = scheduled;
        Triggered = triggered;
    }

    /// <summary>
    ///     Associated run ID.
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    ///     The date when the timer was arranged.
    /// </summary>
    public DateTimeOffset Arranged { get; }

    /// <summary>
    ///     The date when the timer was scheduled.
    /// </summary>
    public DateTimeOffset Scheduled { get; }

    /// <summary>
    ///     The date when the timer was triggered.
    /// </summary>
    public DateTimeOffset Triggered { get; }
}
