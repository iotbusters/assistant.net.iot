using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job create model.
    /// </summary>
    public class JobCreateModel : IMessage<Guid>
    {
        /// <summary>
        ///     Name.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        ///     Trigger type.
        /// </summary>
        public JobTriggerType Trigger { get; set; }

        /// <summary>
        ///     Event mask of <see cref="Trigger"/>.
        /// </summary>
        public IDictionary<string, string>? TriggerEventMask { get; set; }

        /// <summary>
        ///     Type.
        /// </summary>
        public JobType Type { get; set; }

        /// <summary>
        ///     Parameters of <see cref="Type"/>.
        /// </summary>
        public IDictionary<string, string>? Parameters { get; set; } = null!;
    }
}