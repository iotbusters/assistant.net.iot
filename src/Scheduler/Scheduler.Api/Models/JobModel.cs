using Assistant.Net.Scheduler.Api.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job model.
    /// </summary>
    public class JobModel
    {
        /// <summary/>
        public JobModel(
            Guid id,
            string name,
            JobTriggerType trigger,
            IDictionary<string, string>? triggerEventMask,
            JobType type,
            IDictionary<string, string>? parameters)
        {
            Id = id;
            Name = name;
            Trigger = trigger;
            TriggerEventMask = triggerEventMask;
            Type = type;
            Parameters = parameters;
        }

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Name.
        /// </summary>
        [Required]
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