using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Scheduler.Contracts.Models
{
    /// <summary>
    ///     Automation job trigger model.
    /// </summary>
    public class JobTriggerModel : JobModel
    {
        /// <summary/>
        public JobTriggerModel(
            Guid id,
            string name,
            string triggerEventName,
            IDictionary<string, string> triggerEventMask) : base(id, name)
        {
            TriggerEventName = triggerEventName;
            TriggerEventMask = triggerEventMask;
        }

        /// <summary>
        ///     Trigger event name.
        /// </summary>
        public string TriggerEventName { get; }

        /// <summary>
        ///     Event mask of <see cref="TriggerEventName"/>.
        /// </summary>
        [MinLength(1)]
        public IDictionary<string, string> TriggerEventMask { get; }
    }
}
