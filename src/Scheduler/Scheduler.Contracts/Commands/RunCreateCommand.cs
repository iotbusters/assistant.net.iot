using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run creation command.
/// </summary>
public sealed class RunCreateCommand : IMessage<Guid>, INonCaching
{
    /// <summary/>
    public RunCreateCommand(Guid automationId) =>
        AutomationId = automationId;

    /// <summary>
    ///     Automation id.
    /// </summary>
    public Guid AutomationId { get; }
}
