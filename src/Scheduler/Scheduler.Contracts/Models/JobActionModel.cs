using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Models
{
    /// <summary>
    ///     Automation job action model.
    /// </summary>
    public class JobActionModel : JobModel
    {
        /// <summary/>
        public JobActionModel(Guid id, string name, IMessage action) : base(id, name)
        {
            Action = action;
        }

        /// <summary>
        ///     Job action message.
        /// </summary>
        public IMessage Action { get; }
    }
}
