using System;
using System.Collections.Generic;

namespace Assistant.Net.Scheduler.Contracts.Models;

/// <summary>
///     Automation run trigger model.
/// </summary>
public sealed class TriggerModel
{
    /// <summary/>
    public TriggerModel(Guid runId, bool isActive, string triggerEventName, IDictionary<string, string> triggerEventMask)
    {
        RunId = runId;
        IsActive = isActive;
        TriggerEventName = triggerEventName;
        TriggerEventMask = triggerEventMask;
    }

    /// <summary>
    ///     Run id.
    /// </summary>
    public Guid RunId { get; }

    /// <summary>
    ///     Determine if trigger is still active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Event name which trigger is assigned to.
    /// </summary>
    public string TriggerEventName { get; private set; }

    /// <summary>
    ///     Event body mask of <see cref="TriggerEventName"/>.
    /// </summary>
    public IDictionary<string, string> TriggerEventMask { get; private set; }

    /// <summary>
    ///     Activates or disables trigger.
    /// </summary>
    public TriggerModel Activate(bool isActive = true)
    {
        IsActive = isActive;
        return this;
    }
}