using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Api.Models;
using System;

namespace Assistant.Net.Scheduler.Api.Commands
{
    /// <summary>
    ///     Specific automation query.
    /// </summary>
    public class AutomationQuery : IMessage<AutomationModel>
    {
        /// <summary/>
        public AutomationQuery(Guid id) =>
            Id = id;

        /// <summary>
        ///     Unique id.
        /// </summary>
        public Guid Id { get; }
    }
}