using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job deleting command.
/// </summary>
public class JobDeleteCommand : IMessage
{
    /// <summary/>
    public JobDeleteCommand(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}