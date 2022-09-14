using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run trigger deactivation command.
/// </summary>
public sealed class TriggerDeactivateCommand : IMessage
{
    /// <summary/>
    public TriggerDeactivateCommand(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Run id.
    /// </summary>
    public Guid RunId { get; }
}
