using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run starting command.
/// </summary>
public sealed class RunSucceedCommand : IMessage
{
    /// <summary/>
    public RunSucceedCommand(Guid id) =>
        Id = id;

    /// <summary>
    ///     Unique id.
    /// </summary>
    public Guid Id { get; }
}
