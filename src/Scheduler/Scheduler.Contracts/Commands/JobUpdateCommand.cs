using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation job updating command base.
    /// </summary>
    public abstract class JobUpdateCommand : IMessage
    {
        /// <summary/>
        protected JobUpdateCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; }
    }
}
