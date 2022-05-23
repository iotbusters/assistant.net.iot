using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job action creation command.
/// </summary>
public class JobActionUpdateCommand : JobUpdateCommand
{
    /// <summary/>
    public JobActionUpdateCommand(
        Guid id,
        string name,
        IMessage action) : base(id, name)
    {
        Action = action;
    }

    /// <summary>
    ///     Job action message.
    /// </summary>
    public IMessage Action { get; }
}