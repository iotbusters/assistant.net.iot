using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation deleting command.
/// </summary>
public sealed class AutomationDeleteCommand : IMessage
{
    /// <summary/>
    public AutomationDeleteCommand(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
