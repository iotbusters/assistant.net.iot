using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Events
{
    /// <summary>
    ///     Arranged timer of the run has been triggered event.
    /// </summary>
    public class TimerTriggeredEvent : IMessage
    {
        /// <summary>
        ///     Associated run ID.
        /// </summary>
        public Guid RunId { get; set; }
    }
}
