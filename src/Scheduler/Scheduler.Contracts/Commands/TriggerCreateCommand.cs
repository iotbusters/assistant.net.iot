using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation run trigger creation command.
/// </summary>
public sealed class TriggerCreateCommand : IMessage<Guid>
{
    /// <summary/>
    public TriggerCreateCommand(Guid runId) =>
        RunId = runId;

    /// <summary>
    ///     Run id.
    /// </summary>
    public Guid RunId { get; }
}
