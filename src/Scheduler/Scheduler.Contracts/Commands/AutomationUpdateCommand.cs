using Assistant.Net.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation updating command.
    /// </summary>
    public class AutomationUpdateCommand : IMessage
    {
        /// <summary/>
        public AutomationUpdateCommand(Guid id, string name, IEnumerable<JobReferenceDto> jobs)
        {
            Id = id;
            Name = name;
            Jobs = jobs.ToArray();
        }

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Job reference sequence.
        /// </summary>
        public JobReferenceDto[] Jobs { get; }
    }
}
