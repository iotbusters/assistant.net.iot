using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Enums;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Commands
{
    /// <summary>
    ///     Automation job creation command.
    /// </summary>
    public class JobCreateCommand : IMessage<Guid>
    {
        /// <summary/>
        public JobCreateCommand(
            string name,
            JobTriggerType trigger,
            IDictionary<string, string>? triggerEventMask,
            JobType type,
            IDictionary<string, string>? parameters)
        {
            Name = name;
            Trigger = trigger;
            TriggerEventMask = triggerEventMask;
            Type = type;
            Parameters = parameters;
        }
        
        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Trigger type.
        /// </summary>
        public JobTriggerType Trigger { get; }

        /// <summary>
        ///     Event mask of <see cref="Trigger"/>.
        /// </summary>
        public IDictionary<string, string>? TriggerEventMask { get; }

        /// <summary>
        ///     Type.
        /// </summary>
        public JobType Type { get; }

        /// <summary>
        ///     Parameters of <see cref="Type"/>.
        /// </summary>
        public IDictionary<string, string>? Parameters { get; }
    }
}
