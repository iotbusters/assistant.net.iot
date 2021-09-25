using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation run updating command.
    /// </summary>
    public class RunUpdateCommand : IMessage
    {
        /// <summary/>
        public RunUpdateCommand(Guid id, RunStatusDto status)
        {
            Id = id;
            Status = status;
        }

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Latest status of the run.
        /// </summary>
        public RunStatusDto Status { get; }
    }
}
