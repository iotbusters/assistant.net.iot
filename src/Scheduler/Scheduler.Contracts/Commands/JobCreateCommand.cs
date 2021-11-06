using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation job creation command base.
    /// </summary>
    public abstract class JobCreateCommand : IMessage<Guid>
    {
        /// <summary/>
        protected JobCreateCommand(string name) =>
            Name = name;

        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; }
    }
}
