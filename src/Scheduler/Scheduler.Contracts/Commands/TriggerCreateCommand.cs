using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation run trigger deleting command.
    /// </summary>
    public class TriggerCreateCommand : IMessage<Guid>
    {
        /// <summary/>
        public TriggerCreateCommand(Guid runId) =>
            RunId = runId;

        /// <summary>
        ///     Run id.
        /// </summary>
        public Guid RunId { get; }
    }
}
