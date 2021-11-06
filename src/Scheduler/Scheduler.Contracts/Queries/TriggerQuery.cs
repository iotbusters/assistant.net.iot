using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System;

namespace Assistant.Net.Scheduler.Contracts.Queries
{
    /// <summary>
    ///     Specific automation run trigger query.
    /// </summary>
    public class TriggerQuery : IMessage<TriggerModel>
    {
        /// <summary/>
        public TriggerQuery(Guid runId) =>
            RunId = runId;

        /// <summary>
        ///     Automation run id.
        /// </summary>
        public Guid RunId { get; }
    }
}
