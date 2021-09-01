using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using System;

namespace Assistant.Net.Scheduler.Api.Commands
{
    /// <summary>
    ///     Specific automation job query.
    /// </summary>
    public class JobQuery : IMessage<JobModel>
    {
        /// <summary/>
        public JobQuery(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }
    }
}