using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Commands;

/// <summary>
///     Automation job trigger creation command.
/// </summary>
public class JobTriggerCreateCommand : JobCreateCommand
{
    /// <summary/>
    public JobTriggerCreateCommand(
        string name,
        string triggerEventName,
        IDictionary<string, string> triggerEventMask) : base(name)
    {
        TriggerEventName = triggerEventName;
        TriggerEventMask = triggerEventMask;
    }

    /// <summary>
    ///     Trigger event name.
    /// </summary>
    public string TriggerEventName { get; }

    /// <summary>
    ///     Event mask of <see cref="TriggerEventName"/>.
    /// </summary>
    public IDictionary<string, string> TriggerEventMask { get; }
}
