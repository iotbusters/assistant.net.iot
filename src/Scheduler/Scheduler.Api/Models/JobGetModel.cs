using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Scheduler.Api.Models
{
    /// <summary>
    ///     Automation job get model.
    /// </summary>
    public class JobGetModel
    {
        /// <summary/>
        public JobGetModel(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }
    }
}