using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation run creation command.
    /// </summary>
    public class RunCreateCommand : IMessage<Guid>
    {
        /// <summary/>
        public RunCreateCommand(Guid automationId) =>
            AutomationId = automationId;

        /// <summary>
        ///     Automation id.
        /// </summary>
        public Guid AutomationId { get; }
    }
}
