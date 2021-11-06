using Assistant.Net.Messaging.Abstractions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job create model.
    /// </summary>
    public class JobCreateModel
    {
        /// <summary>
        ///     Name.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        ///     Trigger event name.
        /// </summary>
        public string? TriggerEventName { get; set; }

        /// <summary>
        ///     Event mask of <see cref="TriggerEventName"/>.
        /// </summary>
        public IDictionary<string, string>? TriggerEventMask { get; set; }

        /// <summary>
        ///     Job action message to request.
        /// </summary>
        public IMessage? Action { get; set; }
    }
}
