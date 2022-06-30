using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run deleting command.
/// </summary>
public sealed class RunDeleteCommand : IMessage
{
    /// <summary/>
    public RunDeleteCommand(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}