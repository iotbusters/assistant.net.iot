using Assistant.Net.Scheduler.Api.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job update model.
    /// </summary>
    public class JobUpdateModel
    {
        /// <summary/>
        public JobUpdateModel(
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