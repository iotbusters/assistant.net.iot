using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System;

namespace Assistant.Net.Scheduler.Contracts.Queries;

/// <summary>
///     Specific automation job query.
/// </summary>
public class JobQuery : IMessage<JobModel>, IMessageCacheIgnored
{
    /// <summary/>
    public JobQuery(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
