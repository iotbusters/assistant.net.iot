using Assistant.Net.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Scheduler.Api.Commands
{
    /// <summary>
    ///     Automation creation command.
    /// </summary>
    public class AutomationCreateCommand : IMessage<Guid>
    {
        /// <summary/>
        public AutomationCreateCommand(string name, IEnumerable<JobReferenceDto> jobs)
        {
            Name = name;
            Jobs = jobs.ToArray();
        }

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