using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation run trigger updating command.
    /// </summary>
    public class TriggerUpdateCommand : IMessage
    {
        /// <summary/>
        public TriggerUpdateCommand(Guid runId, bool isActive)
        {
            RunId = runId;
            IsActive = isActive;
        }

        /// <summary>
        ///     Run id.
        /// </summary>
        public Guid RunId { get; }

        /// <summary>
        ///     Determines if trigger is still active.
        /// </summary>
        public bool IsActive { get; }
    }
}
