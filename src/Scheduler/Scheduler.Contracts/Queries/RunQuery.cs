using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System;

namespace Assistant.Net.Scheduler.Contracts.Queries;

/// <summary>
///     Specific automation run query.
/// </summary>
public class RunQuery : IMessage<RunModel>, INonCaching
{
    /// <summary/>
    public RunQuery(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
