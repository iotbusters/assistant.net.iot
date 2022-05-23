using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run trigger deleting command.
/// </summary>
public class TriggerDeleteCommand : IMessage
{
    /// <summary/>
    public TriggerDeleteCommand(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Run id.
    /// </summary>
    public Guid RunId { get; }
}