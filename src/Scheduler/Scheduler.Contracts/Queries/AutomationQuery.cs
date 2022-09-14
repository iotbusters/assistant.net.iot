using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Scheduler.Contracts.Models;
using System;

namespace Assistant.Net.Scheduler.Contracts.Queries;

/// <summary>
///     Specific automation query.
/// </summary>
public sealed class AutomationQuery : IMessage<AutomationModel>, INonCaching
{
    /// <summary/>
    public AutomationQuery(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
